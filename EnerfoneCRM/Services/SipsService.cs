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
                    var url = $"{API_URL}?id={Uri.EscapeDataString(cupsNormalizado)}&key={API_KEY}";
                    
                    // Añadir parámetro &g=1 para CUPS de gas
                    if (esGas)
                    {
                        url += "&g=1";
                    }

                    HttpResponseMessage? response = null;
                    string? jsonString = null;
                    try
                    {
                        response = await _httpClient.GetAsync(url);
                        jsonString = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            await GuardarHistoricoAsync(
                                cupsNormalizado,
                                usuarioId,
                                usuarioNombre,
                                usuarioEmail,
                                success: false,
                                httpStatusCode: (int)response.StatusCode,
                                errorMessage: $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}",
                                responseJson: jsonString);

                            response.EnsureSuccessStatusCode();
                        }

                        var datos = DeserializarRespuesta(jsonString);

                        var historicoId = await GuardarHistoricoAsync(
                            cupsNormalizado,
                            usuarioId,
                            usuarioNombre,
                            usuarioEmail,
                            success: true,
                            httpStatusCode: (int)response.StatusCode,
                            errorMessage: null,
                            responseJson: jsonString);

                        var result = new SipsConsultaResult
                        {
                            Respuesta = datos,
                            DesdeCache = false,
                            FechaConsulta = DateTime.Now,
                            HistoricoId = historicoId,
                            Fuente = "API"
                        };

                        _memoryCache.Set(memKey, result, TimeSpan.FromMinutes(memoryCacheMinutes));
                        return result;
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"[SIPS] Error de conexión: {ex.Message}");
                        await GuardarHistoricoAsync(
                            cupsNormalizado,
                            usuarioId,
                            usuarioNombre,
                            usuarioEmail,
                            success: false,
                            httpStatusCode: response != null ? (int?)response.StatusCode : null,
                            errorMessage: ex.Message,
                            responseJson: jsonString);
                        throw new Exception("Error al conectar con el servicio SIPS", ex);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"[SIPS] Error al procesar respuesta: {ex.Message}");
                        await GuardarHistoricoAsync(
                            cupsNormalizado,
                            usuarioId,
                            usuarioNombre,
                            usuarioEmail,
                            success: false,
                            httpStatusCode: response != null ? (int?)response.StatusCode : (int?)HttpStatusCode.OK,
                            errorMessage: ex.Message,
                            responseJson: jsonString);
                        throw new Exception("Error al procesar los datos del servicio SIPS", ex);
                    }
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

        private static SipsResponse? DeserializarRespuesta(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<SipsResponse>(json, options);
        }

        private int? TryGetInt(string key)
        {
            var raw = _configuration[key];
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return int.TryParse(raw, out var value) ? value : null;
        }
    }
}
