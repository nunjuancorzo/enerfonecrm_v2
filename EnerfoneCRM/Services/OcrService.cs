using System.Text;
using System.Text.Json;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

/// <summary>
/// Servicio para realizar OCR en facturas de energía
/// Soporta Azure Document Intelligence, OpenAI Vision, Google Vision y Tesseract OCR (gratuito)
/// </summary>
public class OcrService
{
    private readonly DbContextProvider _dbContextProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public OcrService(DbContextProvider dbContextProvider, IHttpClientFactory httpClientFactory)
    {
        _dbContextProvider = dbContextProvider;
        _httpClientFactory = httpClientFactory;
    }

    public class ResultadoOcr
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public Dictionary<string, string> DatosExtraidos { get; set; } = new();
        public string ProveedorUtilizado { get; set; } = string.Empty;
        public string TextoCompleto { get; set; } = string.Empty;
    }

    /// <summary>
    /// Procesa una factura usando OCR
    /// </summary>
    public async Task<ResultadoOcr> ProcesarFacturaAsync(byte[] archivoBytes, string nombreArchivo, PlantillaPreCarga? plantilla = null)
    {
        try
        {
            Console.WriteLine($"[OCR] ========== INICIO PROCESAMIENTO OCR ==========");
            Console.WriteLine($"[OCR] Archivo: {nombreArchivo}");
            Console.WriteLine($"[OCR] Tamaño: {archivoBytes.Length} bytes");
            Console.WriteLine($"[OCR] Plantilla: {plantilla?.Nombre ?? "Sin plantilla"}");
            
            using var context = _dbContextProvider.CreateDbContext();
            var config = await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();

            if (config == null || string.IsNullOrEmpty(config.OcrProveedor))
            {
                Console.WriteLine($"[OCR] ERROR: No hay configuración de OCR disponible");
                return new ResultadoOcr
                {
                    Exito = false,
                    Mensaje = "No hay configuración de OCR disponible"
                };
            }

            Console.WriteLine($"[OCR] Proveedor principal: {config.OcrProveedor}");
            Console.WriteLine($"[OCR] Proveedor secundario: {config.OcrProveedorSecundario ?? "Ninguno"}");
            Console.WriteLine($"[OCR] Fallback automático: {config.OcrFallbackAutomatico}");

            // Intentar con el proveedor principal
            var resultado = await ProcesarConProveedorAsync(config.OcrProveedor, archivoBytes, nombreArchivo, config, plantilla);

            Console.WriteLine($"[OCR] Resultado del proveedor principal: {(resultado.Exito ? "ÉXITO" : "FALLO")}");
            Console.WriteLine($"[OCR] Mensaje: {resultado.Mensaje}");

            // Si falla y hay fallback configurado
            if (!resultado.Exito && config.OcrFallbackAutomatico && !string.IsNullOrEmpty(config.OcrProveedorSecundario))
            {
                Console.WriteLine($"[OCR] Intentando con proveedor secundario: {config.OcrProveedorSecundario}");
                resultado = await ProcesarConProveedorAsync(config.OcrProveedorSecundario, archivoBytes, nombreArchivo, config, plantilla);
                Console.WriteLine($"[OCR] Resultado del proveedor secundario: {(resultado.Exito ? "ÉXITO" : "FALLO")}");
            }

            Console.WriteLine($"[OCR] ========== FIN PROCESAMIENTO OCR ==========");
            return resultado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OCR] EXCEPCIÓN CRÍTICA: {ex.Message}");
            Console.WriteLine($"[OCR] Stack trace: {ex.StackTrace}");
            return new ResultadoOcr
            {
                Exito = false,
                Mensaje = $"Error al procesar factura: {ex.Message}"
            };
        }
    }

    private async Task<ResultadoOcr> ProcesarConProveedorAsync(
        string proveedor, 
        byte[] archivoBytes, 
        string nombreArchivo, 
        ConfiguracionEmpresa config,
        PlantillaPreCarga? plantilla)
    {
        return proveedor.ToLower() switch
        {
            "azure" => await ProcesarConAzureAsync(archivoBytes, config, plantilla),
            "openai" => await ProcesarConOpenAIAsync(archivoBytes, config, plantilla),
            "google" => await ProcesarConGoogleAsync(archivoBytes, config, plantilla),
            "tesseract" => await ProcesarConTesseractAsync(archivoBytes, nombreArchivo, plantilla),
            _ => new ResultadoOcr { Exito = false, Mensaje = $"Proveedor '{proveedor}' no soportado" }
        };
    }

    private async Task<ResultadoOcr> ProcesarConAzureAsync(byte[] archivoBytes, ConfiguracionEmpresa config, PlantillaPreCarga? plantilla)
    {
        try
        {
            if (string.IsNullOrEmpty(config.OcrEndpoint) || string.IsNullOrEmpty(config.OcrApiKey))
            {
                return new ResultadoOcr { Exito = false, Mensaje = "Configuración de Azure incompleta" };
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(config.OcrTimeout ?? 30);
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.OcrApiKey);

            var content = new ByteArrayContent(archivoBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            var response = await httpClient.PostAsync($"{config.OcrEndpoint}/formrecognizer/documentModels/prebuilt-invoice:analyze?api-version=2023-07-31", content);
            
            if (!response.IsSuccessStatusCode)
            {
                return new ResultadoOcr 
                { 
                    Exito = false, 
                    Mensaje = $"Error en Azure OCR: {response.StatusCode}" 
                };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var datos = ExtraerDatosDeAzure(jsonResult, plantilla);

            return new ResultadoOcr
            {
                Exito = true,
                Mensaje = "Factura procesada correctamente con Azure",
                DatosExtraidos = datos,
                ProveedorUtilizado = "azure"
            };
        }
        catch (Exception ex)
        {
            return new ResultadoOcr
            {
                Exito = false,
                Mensaje = $"Error en Azure OCR: {ex.Message}"
            };
        }
    }

    private async Task<ResultadoOcr> ProcesarConOpenAIAsync(byte[] archivoBytes, ConfiguracionEmpresa config, PlantillaPreCarga? plantilla)
    {
        try
        {
            if (string.IsNullOrEmpty(config.OcrApiKey))
            {
                return new ResultadoOcr { Exito = false, Mensaje = "API Key de OpenAI no configurada" };
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(config.OcrTimeout ?? 30);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.OcrApiKey}");

            // Convertir a base64
            var base64Image = Convert.ToBase64String(archivoBytes);
            var modelo = config.OcrModelo ?? "gpt-4o";

            var prompt = GenerarPromptExtraccion(plantilla);

            var requestBody = new
            {
                model = modelo,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } }
                        }
                    }
                },
                max_tokens = 2000
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ResultadoOcr 
                { 
                    Exito = false, 
                    Mensaje = $"Error en OpenAI: {response.StatusCode} - {errorContent}" 
                };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var datos = ExtraerDatosDeOpenAI(jsonResult);

            return new ResultadoOcr
            {
                Exito = true,
                Mensaje = "Factura procesada correctamente con OpenAI",
                DatosExtraidos = datos,
                ProveedorUtilizado = "openai",
                TextoCompleto = jsonResult
            };
        }
        catch (Exception ex)
        {
            return new ResultadoOcr
            {
                Exito = false,
                Mensaje = $"Error en OpenAI: {ex.Message}"
            };
        }
    }

    private async Task<ResultadoOcr> ProcesarConGoogleAsync(byte[] archivoBytes, ConfiguracionEmpresa config, PlantillaPreCarga? plantilla)
    {
        try
        {
            // TODO: Implementar Google Vision API
            return new ResultadoOcr
            {
                Exito = false,
                Mensaje = "Google Vision aún no implementado"
            };
        }
        catch (Exception ex)
        {
            return new ResultadoOcr
            {
                Exito = false,
                Mensaje = $"Error en Google Vision: {ex.Message}"
            };
        }
    }

    private async Task<ResultadoOcr> ProcesarConTesseractAsync(byte[] archivoBytes, string nombreArchivo, PlantillaPreCarga? plantilla)
    {
        try
        {
            Console.WriteLine($"[OCR] Iniciando procesamiento con Tesseract para archivo: {nombreArchivo}");
            Console.WriteLine($"[OCR] Tamaño del archivo: {archivoBytes.Length} bytes");
            
            // Verificar que Tesseract está instalado
            var tesseractPath = ObtenerRutaTesseract();
            Console.WriteLine($"[OCR] Ruta de Tesseract: {tesseractPath ?? "NO ENCONTRADO"}");
            
            if (string.IsNullOrEmpty(tesseractPath))
            {
                return new ResultadoOcr
                {
                    Exito = false,
                    Mensaje = "Tesseract OCR no está instalado. Ejecute: sudo apt install tesseract-ocr (Linux) o brew install tesseract (macOS)"
                };
            }

            var extension = Path.GetExtension(nombreArchivo).ToLower();
            var archivoTemporal = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}{extension}");
            var archivoSalida = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}");

            Console.WriteLine($"[OCR] Extensión detectada: {extension}");
            Console.WriteLine($"[OCR] Archivo temporal: {archivoTemporal}");
            Console.WriteLine($"[OCR] Archivo salida: {archivoSalida}");

            try
            {
                // Guardar archivo temporal
                await File.WriteAllBytesAsync(archivoTemporal, archivoBytes);
                Console.WriteLine($"[OCR] Archivo temporal guardado correctamente");

                // Si es PDF, convertir a imágenes primero
                if (extension == ".pdf")
                {
                    Console.WriteLine($"[OCR] Convirtiendo PDF a imágenes...");
                    var imagenes = await ConvertirPdfAImagenesAsync(archivoTemporal);
                    if (imagenes == null || imagenes.Count == 0)
                    {
                        Console.WriteLine($"[OCR] ERROR: No se pudo convertir el PDF a imágenes");
                        return new ResultadoOcr
                        {
                            Exito = false,
                            Mensaje = "No se pudo convertir el PDF a imágenes. Instale: sudo apt install poppler-utils (Linux) o brew install poppler (macOS)"
                        };
                    }

                    Console.WriteLine($"[OCR] PDF convertido a {imagenes.Count} imagen(es)");

                    // Procesar cada imagen y combinar resultados
                    var textoCompleto = new StringBuilder();
                    foreach (var imagenPath in imagenes)
                    {
                        Console.WriteLine($"[OCR] Procesando imagen: {imagenPath}");
                        var texto = await EjecutarTesseractAsync(imagenPath, archivoSalida);
                        Console.WriteLine($"[OCR] Texto extraído ({texto.Length} caracteres): {texto.Substring(0, Math.Min(200, texto.Length))}...");
                        textoCompleto.AppendLine(texto);
                        File.Delete(imagenPath);
                    }

                    var textoFinal = textoCompleto.ToString();
                    Console.WriteLine($"[OCR] Texto completo extraído: {textoFinal.Length} caracteres");
                    Console.WriteLine($"[OCR] Extrayendo datos estructurados...");
                    
                    var datos = ExtraerDatosDeTesseractText(textoFinal);
                    Console.WriteLine($"[OCR] Datos extraídos - CUPS: {datos.ContainsKey("cups")} | Comercializadora: {datos.ContainsKey("comercializadora")} | Total: {datos.ContainsKey("total_factura")}");
                    
                    return new ResultadoOcr
                    {
                        Exito = true,
                        Mensaje = "Factura procesada con Tesseract OCR",
                        DatosExtraidos = datos,
                        ProveedorUtilizado = "tesseract",
                        TextoCompleto = textoFinal
                    };
                }
                else
                {
                    Console.WriteLine($"[OCR] Procesando imagen directamente con Tesseract...");
                    // Procesar imagen directamente
                    var texto = await EjecutarTesseractAsync(archivoTemporal, archivoSalida);
                    Console.WriteLine($"[OCR] Texto extraído ({texto.Length} caracteres)");
                    Console.WriteLine($"[OCR] Primeros 500 caracteres: {texto.Substring(0, Math.Min(500, texto.Length))}");
                    
                    var datos = ExtraerDatosDeTesseractText(texto);
                    Console.WriteLine($"[OCR] Datos extraídos - Total campos: {datos.Count}");
                    foreach (var kvp in datos)
                    {
                        Console.WriteLine($"[OCR]   {kvp.Key}: {kvp.Value}");
                    }

                    return new ResultadoOcr
                    {
                        Exito = true,
                        Mensaje = "Factura procesada con Tesseract OCR",
                        DatosExtraidos = datos,
                        ProveedorUtilizado = "tesseract",
                        TextoCompleto = texto
                    };
                }
            }
            finally
            {
                // Limpiar archivos temporales
                if (File.Exists(archivoTemporal))
                {
                    File.Delete(archivoTemporal);
                    Console.WriteLine($"[OCR] Archivo temporal eliminado");
                }
                if (File.Exists(archivoSalida + ".txt"))
                {
                    File.Delete(archivoSalida + ".txt");
                    Console.WriteLine($"[OCR] Archivo salida eliminado");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OCR] EXCEPCIÓN en Tesseract: {ex.Message}");
            Console.WriteLine($"[OCR] Stack trace: {ex.StackTrace}");
            return new ResultadoOcr
            {
                Exito = false,
                Mensaje = $"Error en Tesseract OCR: {ex.Message}"
            };
        }
    }

    private string? ObtenerRutaTesseract()
    {
        // Intentar encontrar tesseract en el PATH
        var rutasPosibles = new[]
        {
            "/usr/bin/tesseract",
            "/usr/local/bin/tesseract",
            "/opt/homebrew/bin/tesseract",
            "C:\\Program Files\\Tesseract-OCR\\tesseract.exe",
            "tesseract" // En PATH
        };

        foreach (var ruta in rutasPosibles)
        {
            try
            {
                var proceso = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ruta,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                proceso.Start();
                proceso.WaitForExit(2000);
                if (proceso.ExitCode == 0)
                    return ruta;
            }
            catch { }
        }

        return null;
    }

    private async Task<List<string>> ConvertirPdfAImagenesAsync(string pdfPath)
    {
        var imagenes = new List<string>();
        var outputDir = Path.GetTempPath();
        var baseNombre = $"pdf_page_{Guid.NewGuid()}";

        try
        {
            // Usar pdftoppm (de poppler-utils) para convertir PDF a imágenes
            var proceso = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pdftoppm",
                    Arguments = $"-png \"{pdfPath}\" \"{Path.Combine(outputDir, baseNombre)}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proceso.Start();
            await proceso.WaitForExitAsync();

            if (proceso.ExitCode == 0)
            {
                // Buscar las imágenes generadas
                var archivos = Directory.GetFiles(outputDir, $"{baseNombre}*.png");
                imagenes.AddRange(archivos);
            }
        }
        catch { }

        return imagenes;
    }

    private async Task<string> EjecutarTesseractAsync(string imagenPath, string archivoSalida)
    {
        try
        {
            var tesseractPath = ObtenerRutaTesseract();
            var proceso = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tesseractPath,
                    Arguments = $"\"{imagenPath}\" \"{archivoSalida}\" -l spa --psm 6",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proceso.Start();
            await proceso.WaitForExitAsync();

            var textoFile = archivoSalida + ".txt";
            if (File.Exists(textoFile))
            {
                var texto = await File.ReadAllTextAsync(textoFile);
                return texto;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    private Dictionary<string, string> ExtraerDatosDeTesseractText(string texto)
    {
        var datos = new Dictionary<string, string>();

        try
        {
            var lineas = texto.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Buscar patrones comunes en facturas
            foreach (var linea in lineas)
            {
                var lineaLimpia = linea.Trim();

                // CUPS (formato ES + 20 dígitos)
                if (System.Text.RegularExpressions.Regex.IsMatch(lineaLimpia, @"ES\d{20}"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(lineaLimpia, @"ES\d{20}");
                    datos["cups"] = match.Value;
                }

                // Total factura (buscar "Total" seguido de número)
                if (lineaLimpia.ToLower().Contains("total") && System.Text.RegularExpressions.Regex.IsMatch(lineaLimpia, @"\d+[.,]\d{2}"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(lineaLimpia, @"\d+[.,]\d{2}");
                    datos["total_factura"] = match.Value.Replace(',', '.');
                }

                // Potencia (buscar "kW")
                if (lineaLimpia.Contains("kW") && System.Text.RegularExpressions.Regex.IsMatch(lineaLimpia, @"\d+[.,]\d{2}"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(lineaLimpia, @"\d+[.,]\d{2}");
                    if (!datos.ContainsKey("potencia_p1"))
                        datos["potencia_p1"] = match.Value.Replace(',', '.');
                }

                // Comercializadoras conocidas
                var comercializadoras = new[] { "Iberdrola", "Endesa", "Naturgy", "Repsol", "TotalEnergies", "Holaluz" };
                foreach (var comercializadora in comercializadoras)
                {
                    if (lineaLimpia.Contains(comercializadora, StringComparison.OrdinalIgnoreCase))
                    {
                        datos["comercializadora_actual"] = comercializadora;
                        break;
                    }
                }

                // Peaje
                var peajes = new[] { "2.0TD", "3.0TD", "6.1TD", "RL.1", "RL.2" };
                foreach (var peaje in peajes)
                {
                    if (lineaLimpia.Contains(peaje))
                    {
                        if (peaje.StartsWith("2.") || peaje.StartsWith("3.") || peaje.StartsWith("6."))
                            datos["peaje_luz"] = peaje;
                        else
                            datos["peaje_gas"] = peaje;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            datos["_error"] = $"Error al extraer datos: {ex.Message}";
        }

        return datos;
    }

    private string GenerarPromptExtraccion(PlantillaPreCarga? plantilla)
    {
        var prompt = @"Extrae la siguiente información de esta factura de energía (luz y/o gas):

DATOS GENERALES:
- CUPS (código de punto de suministro)
- Comercializadora actual
- Tarifa actual
- Peaje de luz (2.0TD, 3.0TD, 6.1TD, etc.)
- Peaje de gas (si aplica)
- Fecha inicio del periodo
- Fecha fin del periodo
- Total de la factura
- IVA
- Tipo de energía (LUZ, GAS o LUZ+GAS)

DATOS DE LUZ (si aplica):
- Potencia contratada P1, P2, P3, P4, P5, P6 (en kW)
- Precio potencia P1, P2, P3, P4, P5, P6 (€/kW/día o €/kW/mes)
- Consumo energía P1, P2, P3, P4, P5, P6 (en kWh)
- Precio energía P1, P2, P3, P4, P5, P6 (€/kWh)

DATOS DE GAS (si aplica):
- Consumo de gas (en kWh)
- Término fijo gas (€/día)
- Término variable gas (€/kWh)

Devuelve la información en formato JSON con las claves en minúsculas y snake_case.
Si un campo no está presente, omítelo o ponlo como null.
Ejemplo: {""cups"": ""ES0031..."", ""total_factura"": 85.50, ""potencia_p1"": 3.45, ...}";

        return prompt;
    }

    private Dictionary<string, string> ExtraerDatosDeAzure(string jsonResult, PlantillaPreCarga? plantilla)
    {
        var datos = new Dictionary<string, string>();
        
        try
        {
            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;

            if (root.TryGetProperty("analyzeResult", out var analyzeResult) &&
                analyzeResult.TryGetProperty("documents", out var documents))
            {
                foreach (var document in documents.EnumerateArray())
                {
                    if (document.TryGetProperty("fields", out var fields))
                    {
                        // Mapear campos comunes de facturas
                        ExtraerCampoAzure(fields, "InvoiceTotal", datos, "total_factura");
                        ExtraerCampoAzure(fields, "InvoiceId", datos, "numero_factura");
                        ExtraerCampoAzure(fields, "InvoiceDate", datos, "fecha_factura");
                        ExtraerCampoAzure(fields, "DueDate", datos, "fecha_vencimiento");
                        ExtraerCampoAzure(fields, "VendorName", datos, "comercializadora_actual");
                        ExtraerCampoAzure(fields, "CustomerName", datos, "nombre_cliente");
                        // Agregar más campos según necesidad
                    }
                }
            }
        }
        catch (Exception ex)
        {
            datos["_error"] = $"Error al parsear respuesta de Azure: {ex.Message}";
        }

        return datos;
    }

    private void ExtraerCampoAzure(JsonElement fields, string campoAzure, Dictionary<string, string> datos, string campoDestino)
    {
        if (fields.TryGetProperty(campoAzure, out var campo) &&
            campo.TryGetProperty("valueString", out var valor))
        {
            datos[campoDestino] = valor.GetString() ?? "";
        }
        else if (campo.TryGetProperty("valueNumber", out var valorNum))
        {
            datos[campoDestino] = valorNum.GetDecimal().ToString();
        }
    }

    private Dictionary<string, string> ExtraerDatosDeOpenAI(string jsonResult)
    {
        var datos = new Dictionary<string, string>();
        
        try
        {
            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;

            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content))
                {
                    var contentText = content.GetString() ?? "";
                    
                    // Intentar parsear como JSON
                    try
                    {
                        var cleanJson = ExtractJsonFromText(contentText);
                        using var datosDoc = JsonDocument.Parse(cleanJson);
                        var datosRoot = datosDoc.RootElement;

                        foreach (var propiedad in datosRoot.EnumerateObject())
                        {
                            datos[propiedad.Name] = propiedad.Value.ToString();
                        }
                    }
                    catch
                    {
                        datos["_raw_content"] = contentText;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            datos["_error"] = $"Error al parsear respuesta de OpenAI: {ex.Message}";
        }

        return datos;
    }

    private string ExtractJsonFromText(string text)
    {
        // Buscar JSON entre ```json y ``` o solo entre { y }
        var startIndex = text.IndexOf("{");
        var endIndex = text.LastIndexOf("}");
        
        if (startIndex >= 0 && endIndex > startIndex)
        {
            return text.Substring(startIndex, endIndex - startIndex + 1);
        }
        
        return text;
    }
}
