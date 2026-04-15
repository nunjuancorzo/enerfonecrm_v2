using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EnerfoneCRM.Services
{
    public class SipsService
    {
        private readonly HttpClient _httpClient;
        private readonly DbContextProvider _dbContextProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        private const string API_URL = "http://35.181.7.83/api/sips2.php";
        private const string API_KEY = "OPT7393";

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cupsLocks = new();

        public SipsService(
            HttpClient httpClient,
            DbContextProvider dbContextProvider,
            IMemoryCache memoryCache,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _dbContextProvider = dbContextProvider;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        /// <summary>
        /// Método legacy: mantiene compatibilidad con el uso existente.
        /// Internamente usa histórico/cache para ahorrar llamadas a la API.
        /// </summary>
        public async Task<SipsResponse?> ObtenerDatosCupsAsync(string cups)
        {
            var result = await ObtenerDatosCupsConCacheAsync(cups);
            return result.Respuesta;
        }

        /// <summary>
        /// Obtiene datos SIPS usando primero cache (memoria + BD) y solo llama a la API si no hay histórico.
        /// - Sips:CacheMaxAgeDays: si existe, solo usa histórico más nuevo que N días.
        /// - Sips:MemoryCacheMinutes: TTL del cache en memoria (por defecto 60).
        /// </summary>
        public async Task<SipsConsultaResult> ObtenerDatosCupsConCacheAsync(
            string cups,
            int? usuarioId = null,
            string? usuarioNombre = null,
            string? usuarioEmail = null,
            bool forzarApi = false,
            bool esGas = false)
        {
            try
            {
                var cupsNormalizado = NormalizarCups(cups);
                if (string.IsNullOrWhiteSpace(cupsNormalizado))
                {
                    throw new ArgumentException("El CUPS no puede estar vacío");
                }

                var maxCacheAgeDays = TryGetInt("Sips:CacheMaxAgeDays");
                var memoryCacheMinutes = TryGetInt("Sips:MemoryCacheMinutes") ?? 60;
                var monthlyQuota = TryGetInt("Sips:MonthlyQuota");
                var negativeCacheHours = TryGetInt("Sips:NegativeCacheHours") ?? 24;

                var memKey = $"sips:{cupsNormalizado}";

                // 1) Cache en memoria
                if (!forzarApi && _memoryCache.TryGetValue<SipsConsultaResult>(memKey, out var cached) && cached.Respuesta != null)
                {
                    return cached;
                }

                // 2) Lock por CUPS para evitar llamadas duplicadas simultáneas
                var sem = _cupsLocks.GetOrAdd(cupsNormalizado, _ => new SemaphoreSlim(1, 1));
                await sem.WaitAsync();
                try
                {
                    // Re-check memoria tras adquirir lock
                    if (!forzarApi && _memoryCache.TryGetValue<SipsConsultaResult>(memKey, out cached) && cached.Respuesta != null)
                    {
                        return cached;
                    }

                    // 3) Histórico en BD
                    if (!forzarApi)
                    {
                        await using var context = _dbContextProvider.CreateDbContext();

                        // 3.a) Cache negativa (solo errores 4xx recientes) para evitar pagar por CUPS inválidos repetidos
                        var negativeSince = DateTime.Now.AddHours(-negativeCacheHours);
                        var ultimoError4xx = await context.HistoricoSipsConsultas
                            .AsNoTracking()
                            .Where(h => h.Cups == cupsNormalizado
                                        && !h.Success
                                        && h.HttpStatusCode >= 400
                                        && h.HttpStatusCode < 500
                                        && h.FechaConsulta >= negativeSince)
                            .OrderByDescending(h => h.FechaConsulta)
                            .FirstOrDefaultAsync();

                        if (ultimoError4xx != null)
                        {
                            throw new Exception($"Consulta evitada para ahorrar llamadas: el CUPS ya falló recientemente (HTTP {ultimoError4xx.HttpStatusCode})." +
                                                (string.IsNullOrWhiteSpace(ultimoError4xx.ErrorMessage) ? "" : $" Detalle: {ultimoError4xx.ErrorMessage}"));
                        }

                        var query = context.HistoricoSipsConsultas
                            .AsNoTracking()
                            .Where(h => h.Cups == cupsNormalizado && h.Success && h.ResponseJson != null)
                            .OrderByDescending(h => h.FechaConsulta);

                        if (maxCacheAgeDays.HasValue)
                        {
                            var limite = DateTime.Now.AddDays(-maxCacheAgeDays.Value);
                            query = query.Where(h => h.FechaConsulta >= limite).OrderByDescending(h => h.FechaConsulta);
                        }

                        var ultimo = await query.FirstOrDefaultAsync();
                        if (ultimo?.ResponseJson != null)
                        {
                            var respuestaHistorica = DeserializarRespuesta(ultimo.ResponseJson);
                            var fromDb = new SipsConsultaResult
                            {
                                Respuesta = respuestaHistorica,
                                DesdeCache = true,
                                FechaConsulta = ultimo.FechaConsulta,
                                HistoricoId = ultimo.Id,
                                Fuente = "HISTORICO"
                            };

                            _memoryCache.Set(memKey, fromDb, TimeSpan.FromMinutes(memoryCacheMinutes));
                            return fromDb;
                        }
                    }

                    // 3.b) Cuota mensual (solo cuenta llamadas reales a API, porque solo esas se guardan aquí)
                    if (monthlyQuota.HasValue && monthlyQuota.Value > 0)
                    {
                        var used = await ObtenerConsumoMesActualAsync();
                        if (used >= monthlyQuota.Value)
                        {
                            throw new Exception($"Se ha alcanzado el límite mensual de consultas SIPS ({used}/{monthlyQuota.Value}). Solo se permite servir desde histórico.");
                        }
                    }

                    // 4) Llamada a API (de pago)
                    SipsResponse? datos;
                    
                    if (esGas)
                    {
                        // Gas: hacer dos llamadas a sips3.php
                        // 1. id=3 para información del suministro
                        var urlSuministro = $"http://35.181.7.83/api/sips3.php?id=3&cups={Uri.EscapeDataString(cupsNormalizado)}";
                        Console.WriteLine($"[SIPS GAS] Consultando API suministro: {urlSuministro}");
                        
                        var csvSuministro = await ConsultarApiAsync(urlSuministro);
                        datos = ParsearCsv(csvSuministro);
                        
                        // 2. id=4 para consumos históricos
                        try
                        {
                            var urlConsumos = $"http://35.181.7.83/api/sips3.php?id=4&cups={Uri.EscapeDataString(cupsNormalizado)}";
                            Console.WriteLine($"[SIPS GAS] Consultando API consumos: {urlConsumos}");
                            
                            var csvConsumos = await ConsultarApiAsync(urlConsumos);
                            Console.WriteLine($"[SIPS GAS] Respuesta consumos (primeros 500 chars): {(csvConsumos?.Length > 500 ? csvConsumos.Substring(0, 500) : csvConsumos)}");
                            
                            // Verificar si hay consumos
                            if (!string.IsNullOrWhiteSpace(csvConsumos) && !csvConsumos.Contains("No se encontraron"))
                            {
                                Console.WriteLine($"[SIPS GAS] Intentando parsear CSV de consumos...");
                                var consumos = ParsearCsvConsumos(csvConsumos);
                                Console.WriteLine($"[SIPS GAS] Consumos parseados: {consumos?.Count ?? 0}");
                                
                                if (datos != null && consumos != null && consumos.Any())
                                {
                                    datos.ConsumosSips = consumos;
                                    Console.WriteLine($"[SIPS GAS] ✓ Se obtuvieron {consumos.Count} registros de consumo");
                                }
                                else
                                {
                                    Console.WriteLine($"[SIPS GAS] ⚠️ El parseo no devolvió consumos (lista vacía o null)");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[SIPS GAS] ⚠️ No hay consumos históricos disponibles (respuesta vacía o mensaje 'No se encontraron')");
                            }
                        }
                        catch (Exception exConsumos)
                        {
                            Console.WriteLine($"[SIPS GAS] ❌ Error obteniendo consumos (no crítico): {exConsumos.Message}");
                            Console.WriteLine($"[SIPS GAS] Stack trace: {exConsumos.StackTrace}");
                            // No fallar si no hay consumos, solo no mostrar gráficos
                        }
                        
                        // Guardar respuesta completa (suministro + consumos) como JSON
                        var responseJsonCompleto = SerializarRespuesta(datos);
                        await GuardarHistoricoAsync(
                            cupsNormalizado,
                            usuarioId,
                            usuarioNombre,
                            usuarioEmail,
                            success: true,
                            httpStatusCode: 200,
                            errorMessage: null,
                            responseJson: responseJsonCompleto);
                    }
                    else
                    {
                        // Luz: hacer dos llamadas a sips3.php
                        // 1. id=1 para información del cliente
                        string cupsParaConsulta = cupsNormalizado;
                        bool intentoConSufijo = false;
                        
                        // Intentar primero con el CUPS original
                        var urlCliente = $"http://35.181.7.83/api/sips3.php?id=1&cups={Uri.EscapeDataString(cupsParaConsulta)}";
                        Console.WriteLine($"[SIPS LUZ] Consultando API cliente: {urlCliente}");
                        
                        try
                        {
                            var csvCliente = await ConsultarApiAsync(urlCliente);
                            datos = ParsearCsv(csvCliente);
                            
                            // Verificar si los datos están completos
                            if (datos == null || datos.ClientesSips == null || !datos.ClientesSips.Any() || 
                                string.IsNullOrEmpty(datos.ClientesSips[0]?.CodigoCUPS))
                            {
                                throw new Exception("Respuesta vacía o incompleta");
                            }
                        }
                        catch (Exception exCliente)
                        {
                            Console.WriteLine($"[SIPS LUZ] Error con CUPS original: {exCliente.Message}");
                            
                            // Si el CUPS no termina en "0F", intentar agregándolo
                            if (!cupsNormalizado.EndsWith("0F", StringComparison.OrdinalIgnoreCase))
                            {
                                cupsParaConsulta = cupsNormalizado + "0F";
                                urlCliente = $"http://35.181.7.83/api/sips3.php?id=1&cups={Uri.EscapeDataString(cupsParaConsulta)}";
                                Console.WriteLine($"[SIPS LUZ] Reintentando con sufijo 0F: {urlCliente}");
                                intentoConSufijo = true;
                                
                                var csvCliente = await ConsultarApiAsync(urlCliente);
                                datos = ParsearCsv(csvCliente);
                            }
                            else
                            {
                                // Si ya termina en 0F y falló, propagar el error
                                throw;
                            }
                        }
                        
                        // 2. id=2 para consumos históricos (usar el mismo CUPS que funcionó)
                        try
                        {
                            var urlConsumos = $"http://35.181.7.83/api/sips3.php?id=2&cups={Uri.EscapeDataString(cupsParaConsulta)}";
                            Console.WriteLine($"[SIPS LUZ] Consultando API consumos: {urlConsumos}");
                            
                            var csvConsumos = await ConsultarApiAsync(urlConsumos);
                            Console.WriteLine($"[SIPS LUZ] Respuesta consumos (primeros 500 chars): {(csvConsumos?.Length > 500 ? csvConsumos.Substring(0, 500) : csvConsumos)}");
                            
                            // Verificar si hay consumos
                            if (!string.IsNullOrWhiteSpace(csvConsumos) && !csvConsumos.Contains("No se encontraron"))
                            {
                                Console.WriteLine($"[SIPS LUZ] Intentando parsear CSV de consumos...");
                                var consumos = ParsearCsvConsumos(csvConsumos);
                                Console.WriteLine($"[SIPS LUZ] Consumos parseados: {consumos?.Count ?? 0}");
                                
                                if (datos != null && consumos != null && consumos.Any())
                                {
                                    datos.ConsumosSips = consumos;
                                    Console.WriteLine($"[SIPS LUZ] ✓ Se obtuvieron {consumos.Count} registros de consumo");
                                }
                                else
                                {
                                    Console.WriteLine($"[SIPS LUZ] ⚠️ El parseo no devolvió consumos (lista vacía o null)");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[SIPS LUZ] ⚠️ No hay consumos históricos disponibles (respuesta vacía o mensaje 'No se encontraron')");
                            }
                        }
                        catch (Exception exConsumos)
                        {
                            Console.WriteLine($"[SIPS LUZ] ❌ Error obteniendo consumos (no crítico): {exConsumos.Message}");
                            Console.WriteLine($"[SIPS LUZ] Stack trace: {exConsumos.StackTrace}");
                            // No fallar si no hay consumos, solo no mostrar gráficos
                        }
                        
                        // Guardar respuesta completa (cliente + consumos) como JSON
                        // Si se usó el sufijo 0F, guardarlo con el CUPS que funcionó
                        var responseJsonCompleto = SerializarRespuesta(datos);
                        await GuardarHistoricoAsync(
                            intentoConSufijo ? cupsParaConsulta : cupsNormalizado,
                            usuarioId,
                            usuarioNombre,
                            usuarioEmail,
                            success: true,
                            httpStatusCode: 200,
                            errorMessage: null,
                            responseJson: responseJsonCompleto);
                    }

                    var result = new SipsConsultaResult
                    {
                        Respuesta = datos,
                        DesdeCache = false,
                        FechaConsulta = DateTime.Now,
                        HistoricoId = null,
                        Fuente = "API"
                    };

                    _memoryCache.Set(memKey, result, TimeSpan.FromMinutes(memoryCacheMinutes));
                    return result;
                }
                finally
                {
                    sem.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SIPS] Error inesperado: {ex.Message}");
                throw;
            }
        }

        public async Task<List<HistoricoSipsConsulta>> ObtenerUltimasConsultasAsync(int take = 200)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.HistoricoSipsConsultas
                .AsNoTracking()
                .OrderByDescending(h => h.FechaConsulta)
                .Take(take)
                .ToListAsync();
        }

        public async Task<(int Used, int? Quota)> ObtenerCuotaMesActualAsync()
        {
            var quota = TryGetInt("Sips:MonthlyQuota");
            var used = await ObtenerConsumoMesActualAsync();
            return (used, quota);
        }

        private async Task<int> ObtenerConsumoMesActualAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
            var next = start.AddMonths(1);

            // Cada fila aquí corresponde a una llamada real a la API (no guardamos cuando se sirve desde histórico)
            return await context.HistoricoSipsConsultas
                .AsNoTracking()
                .CountAsync(h => h.FechaConsulta >= start && h.FechaConsulta < next);
        }

        private async Task<int?> GuardarHistoricoAsync(
            string cupsNormalizado,
            int? usuarioId,
            string? usuarioNombre,
            string? usuarioEmail,
            bool success,
            int? httpStatusCode,
            string? errorMessage,
            string? responseJson)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();

                var row = new HistoricoSipsConsulta
                {
                    Cups = cupsNormalizado,
                    UsuarioId = usuarioId,
                    UsuarioNombre = usuarioNombre,
                    UsuarioEmail = usuarioEmail,
                    FechaConsulta = DateTime.Now,
                    Success = success,
                    HttpStatusCode = httpStatusCode,
                    ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage,
                    ResponseJson = responseJson,
                    ResponseSize = responseJson != null ? responseJson.Length : null
                };

                context.HistoricoSipsConsultas.Add(row);
                await context.SaveChangesAsync();
                return row.Id;
            }
            catch (Exception ex)
            {
                // No romper la consulta por un fallo al guardar histórico.
                Console.WriteLine($"[SIPS] No se pudo guardar histórico: {ex.Message}");
                return null;
            }
        }

        private static string NormalizarCups(string cups)
        {
            return (cups ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static SipsResponse? DeserializarRespuesta(string contenido)
        {
            // Detectar si es CSV o JSON
            if (contenido.TrimStart().StartsWith("{") || contenido.TrimStart().StartsWith("["))
            {
                // Es JSON - usar deserialización JSON
                return DeserializarJson(contenido);
            }
            else
            {
                // Es CSV - parsear CSV
                return ParsearCsv(contenido);
            }
        }

        private static string SerializarRespuesta(SipsResponse? respuesta)
        {
            if (respuesta == null) return "{}";
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };
            
            return JsonSerializer.Serialize(respuesta, options);
        }

        private static SipsResponse? DeserializarJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                AllowTrailingCommas = true
            };

            try
            {
                return JsonSerializer.Deserialize<SipsResponse>(json, options);
            }
            catch (JsonException ex)
            {
                // Log adicional del contenido para facilitar debug
                var jsonPreview = json.Length > 500 ? json.Substring(0, 500) + "..." : json;
                Console.WriteLine($"[SIPS] DeserializarRespuesta - JSON problemático: {jsonPreview}");
                throw;
            }
        }

        private static SipsResponse? ParsearCsv(string csv)
        {
            try
            {
                var lineas = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lineas.Length < 2)
                {
                    throw new Exception($"CSV inválido: esperadas al menos 2 líneas, encontradas {lineas.Length}");
                }

                // Primera línea: cabeceras
                var cabeceras = ParsearLineaCsv(lineas[0]);
                // Segunda línea (y posiblemente más): datos
                var valores = ParsearLineaCsv(lineas[1]);

                Console.WriteLine($"[SIPS CSV] Cabeceras parseadas: {cabeceras.Count}");
                Console.WriteLine($"[SIPS CSV] Valores parseados: {valores.Count}");

                if (cabeceras.Count != valores.Count)
                {
                    throw new Exception($"CSV inválido: {cabeceras.Count} cabeceras vs {valores.Count} valores");
                }

                // Crear diccionario campo -> valor
                var datos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < cabeceras.Count; i++)
                {
                    datos[cabeceras[i]] = valores[i];
                }

                // Mapear a ClienteSips
                var cliente = new ClienteSips
                {
                    CodigoCUPS = ObtenerValor(datos, "cups"),
                    CodigoEmpresaDistribuidora = ObtenerValor(datos, "codigoEmpresaDistribuidora"),
                    NombreEmpresaDistribuidora = ObtenerValor(datos, "nombreEmpresaDistribuidora"),
                    CodigoPostalPS = ObtenerValor(datos, "codigoPostalPS"),
                    MunicipioPS = ObtenerValor(datos, "municipioPS"),
                    CodigoProvinciaPS = ObtenerValor(datos, "codigoProvinciaPS"),
                    FechaAltaSuministro = ParsearFecha(ObtenerValor(datos, "fechaAltaSuministro")),
                    CodigoTarifaATREnVigor = ObtenerValor(datos, "codigoTarifaATREnVigor"),
                    CodigoTensionV = ObtenerValor(datos, "codigoTensionV"),
                    PotenciaMaximaBIEW = ParsearDecimal(ObtenerValor(datos, "potenciaMaximaBIEW")),
                    PotenciaMaximaAPMW = ParsearDecimal(ObtenerValor(datos, "potenciaMaximaAPMW")),
                    CodigoClasificacionPS = ObtenerValor(datos, "codigoClasificacionPS"),
                    CodigoDisponibilidadICP = ObtenerValor(datos, "codigoDisponibilidadICP"),
                    TipoPerfilConsumo = ObtenerValor(datos, "tipoPerfilConsumo"),
                    ValorDerechosExtensionW = ObtenerValor(datos, "valorDerechosExtensionW"),
                    ValorDerechosAccesoW = ObtenerValor(datos, "valorDerechosAccesoW"),
                    CodigoPropiedadEquipoMedida = ObtenerValor(datos, "codigoPropiedadEquipoMedida"),
                    CodigoPropiedadICP = ObtenerValor(datos, "codigoPropiedadICP"),
                    PotenciasContratadasEnWP1 = ParsearDecimalKw(ObtenerValor(datos, "potenciasContratadasEnWP1")),
                    PotenciasContratadasEnWP2 = ParsearDecimalKw(ObtenerValor(datos, "potenciasContratadasEnWP2")),
                    PotenciasContratadasEnWP3 = ParsearDecimalKw(ObtenerValor(datos, "potenciasContratadasEnWP3")),
                    PotenciasContratadasEnWP4 = ParsearDecimalKw(ObtenerValor(datos, "potenciasContratadasEnWP4")),
                    PotenciasContratadasEnWP5 = ParsearDecimalKw(ObtenerValor(datos, "potenciasContratadasEnWP5")),
                    PotenciasContratadasEnWP6 = ParsearDecimalKw(ObtenerValor(datos, "potenciasContratadasEnWP6")),
                    FechaUltimoMovimientoContrato = ParsearFecha(ObtenerValor(datos, "fechaUltimoMovimientoContrato")),
                    FechaUltimoCambioComercializador = ParsearFecha(ObtenerValor(datos, "fechaUltimoCambioComercializador")),
                    FechaLimiteDerechosReconocidos = ParsearFecha(ObtenerValor(datos, "fechaLimiteDerechosReconocidos")),
                    FechaUltimaLectura = ParsearFecha(ObtenerValor(datos, "fechaUltimaLectura")),
                    InformacionImpagos = ObtenerValor(datos, "informacionImpagos"),
                    ImporteDepositoGarantiaEuros = ObtenerValor(datos, "importeDepositoGarantiaEuros"),
                    TipoIdTitular = ObtenerValor(datos, "tipoIdTitular"),
                    EsViviendaHabitual = ObtenerValor(datos, "esViviendaHabitual"),
                    CodigoComercializadora = ObtenerValor(datos, "codigoComercializadora"),
                    CodigoTelegestion = ObtenerValor(datos, "codigoTelegestion"),
                    CodigoFasesEquipoMedida = ObtenerValor(datos, "codigoFasesEquipoMedida"),
                    CodigoAutoconsumo = ObtenerValor(datos, "codigoAutoconsumo"),
                    CodigoTipoContrato = ObtenerValor(datos, "codigoTipoContrato"),
                    CodigoPeriodicidadFacturacion = ObtenerValor(datos, "codigoPeriodicidadFacturacion"),
                    Cnae = ObtenerValor(datos, "CNAE"),
                    AplicacionBonoSocial = ObtenerValor(datos, "aplicacionBonoSocial")
                };

                return new SipsResponse
                {
                    ClientesSips = new List<ClienteSips> { cliente },
                    ConsumosSips = null,
                    DatosTitular = null
                };
            }
            catch (Exception ex)
            {
                var csvPreview = csv.Length > 500 ? csv.Substring(0, 500) + "..." : csv;
                Console.WriteLine($"[SIPS] Error parseando CSV: {ex.Message}");
                Console.WriteLine($"[SIPS] CSV problemático: {csvPreview}");
                throw;
            }
        }

        /// <summary>
        /// Parsea una línea CSV respetando comillas y comas dentro de valores entrecomillados.
        /// Ejemplo: "campo1","campo con, coma","campo3" -> ["campo1", "campo con, coma", "campo3"]
        /// </summary>
        private static List<string> ParsearLineaCsv(string linea)
        {
            var campos = new List<string>();
            var campoActual = new System.Text.StringBuilder();
            var dentroDeComillas = false;
            var i = 0;

            while (i < linea.Length)
            {
                var c = linea[i];

                if (c == '"')
                {
                    if (dentroDeComillas && i + 1 < linea.Length && linea[i + 1] == '"')
                    {
                        // Comilla doble escapada ("")
                        campoActual.Append('"');
                        i += 2;
                        continue;
                    }
                    else
                    {
                        // Toggle estado de comillas
                        dentroDeComillas = !dentroDeComillas;
                        i++;
                        continue;
                    }
                }

                if (c == ',' && !dentroDeComillas)
                {
                    // Fin del campo
                    campos.Add(campoActual.ToString().Trim());
                    campoActual.Clear();
                    i++;
                    continue;
                }

                // Carácter normal
                campoActual.Append(c);
                i++;
            }

            // Agregar el último campo
            campos.Add(campoActual.ToString().Trim());

            return campos;
        }

        private static string? ObtenerValor(Dictionary<string, string> datos, string clave)
        {
            if (datos.TryGetValue(clave, out var valor) && !string.IsNullOrWhiteSpace(valor))
            {
                return valor;
            }
            return null;
        }

        private static DateTime? ParsearFecha(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            if (DateTime.TryParseExact(valor, "yyyy-MM-dd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var fecha))
            {
                return fecha;
            }

            if (DateTime.TryParse(valor, out fecha))
            {
                return fecha;
            }

            return null;
        }

        private static decimal? ParsearDecimal(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            if (decimal.TryParse(valor, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out var resultado))
            {
                return resultado;
            }

            return null;
        }

        private static decimal? ParsearDecimalKw(string? valor)
        {
            var valorWatts = ParsearDecimal(valor);
            // Convertir de Watts a kW
            return valorWatts.HasValue ? valorWatts.Value / 1000m : null;
        }

        /// <summary>
        /// Realiza una llamada HTTP a la API especificada y devuelve el contenido como string
        /// </summary>
        private async Task<string> ConsultarApiAsync(string url)
        {
            HttpResponseMessage? response = null;
            string? contenido = null;
            
            try
            {
                response = await _httpClient.GetAsync(url);
                contenido = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"[SIPS API] HTTP Status: {(int)response.StatusCode}");
                Console.WriteLine($"[SIPS API] Response length: {contenido?.Length ?? 0} caracteres");
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                }
                
                if (string.IsNullOrWhiteSpace(contenido))
                {
                    throw new Exception("La API devolvió una respuesta vacía");
                }
                
                return contenido;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[SIPS API] Error de conexión: {ex.Message}");
                throw new Exception($"Error al conectar con SIPS: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parsea un CSV de consumos de electricidad y devuelve una lista de ConsumoSips
        /// </summary>
        private static List<ConsumoSips> ParsearCsvConsumos(string csv)
        {
            try
            {
                var lineas = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                Console.WriteLine($"[SIPS CONSUMOS] Total de líneas en CSV: {lineas.Length}");
                
                if (lineas.Length < 2)
                {
                    Console.WriteLine($"[SIPS CONSUMOS] ⚠️ CSV sin datos de consumo (menos de 2 líneas)");
                    return new List<ConsumoSips>();
                }

                // Primera línea: cabeceras
                var cabeceras = ParsearLineaCsv(lineas[0]);
                Console.WriteLine($"[SIPS CONSUMOS] Cabeceras encontradas ({cabeceras.Count}): {string.Join(", ", cabeceras.Take(10))}");
                
                var consumos = new List<ConsumoSips>();

                // Procesar cada línea de datos (desde la línea 1 en adelante)
                for (int i = 1; i < lineas.Length; i++)
                {
                    try
                    {
                        var valores = ParsearLineaCsv(lineas[i]);
                        
                        if (valores.Count != cabeceras.Count)
                        {
                            Console.WriteLine($"[SIPS CONSUMOS] ⚠️ Línea {i} ignorada: {cabeceras.Count} cabeceras vs {valores.Count} valores");
                            continue;
                        }

                        var datos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        for (int j = 0; j < cabeceras.Count; j++)
                        {
                            datos[cabeceras[j]] = valores[j];
                        }

                        var consumo = new ConsumoSips
                        {
                            CodigoCUPS = ObtenerValor(datos, "cups"),
                            FechaInicio = ParsearFecha(ObtenerValor(datos, "fechaInicioMesConsumo")),
                            FechaFin = ParsearFecha(ObtenerValor(datos, "fechaFinMesConsumo")),
                            CodigoTarifaATR = ObtenerValor(datos, "codigoTarifaATR"),
                            Activa1 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaActivaEnWhP1")),
                            Activa2 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaActivaEnWhP2")),
                            Activa3 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaActivaEnWhP3")),
                            Activa4 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaActivaEnWhP4")),
                            Activa5 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaActivaEnWhP5")),
                            Activa6 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaActivaEnWhP6")),
                            Reactiva1 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaReactivaEnVArhP1")),
                            Reactiva2 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaReactivaEnVArhP2")),
                            Reactiva3 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaReactivaEnVArhP3")),
                            Reactiva4 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaReactivaEnVArhP4")),
                            Reactiva5 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaReactivaEnVArhP5")),
                            Reactiva6 = ParsearDecimalKwh(ObtenerValor(datos, "consumoEnergiaReactivaEnVArhP6"))
                        };

                        // Solo agregar si tiene fechas válidas
                        if (consumo.FechaInicio.HasValue && consumo.FechaFin.HasValue)
                        {
                            consumos.Add(consumo);
                        }
                        else
                        {
                            Console.WriteLine($"[SIPS CONSUMOS] ⚠️ Línea {i} ignorada: fechas inválidas (Inicio={consumo.FechaInicio}, Fin={consumo.FechaFin})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SIPS CONSUMOS] ❌ Error procesando línea {i}: {ex.Message}");
                    }
                }

                Console.WriteLine($"[SIPS CONSUMOS] ✓ Total parseados exitosamente: {consumos.Count} registros de consumo");
                return consumos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SIPS CONSUMOS] ❌ Error crítico parseando CSV: {ex.Message}");
                return new List<ConsumoSips>();
            }
        }

        /// <summary>
        /// Convierte Wh (vatio-hora) a kWh (kilovatio-hora)
        /// </summary>
        private static decimal? ParsearDecimalKwh(string? valor)
        {
            var valorWh = ParsearDecimal(valor);
            // Convertir de Wh a kWh
            return valorWh.HasValue ? valorWh.Value / 1000m : null;
        }

        private int? TryGetInt(string key)
        {
            var raw = _configuration[key];
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return int.TryParse(raw, out var value) ? value : null;
        }
    }
}
