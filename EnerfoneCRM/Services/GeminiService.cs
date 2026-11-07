using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace EnerfoneCRM.Services;

public class GeminiService(HttpClient httpClient, IConfiguration configuration, GeminiConfiguracionService? configuracionPrivada = null,
    AuthService? auth = null, IMemoryCache? cache = null)
{
    public const string ModeloPredeterminado = "gemini-3.1-flash-lite";
    private sealed class Cuota { public int Peticiones; }
    public bool Disponible => configuracionPrivada?.Configurado ?? !string.IsNullOrWhiteSpace(configuration["Gemini:ApiKey"] ?? configuration["GEMINI_API_KEY"]);

    public async Task<IReadOnlyList<string>> ObtenerModelosAsync(CancellationToken cancellationToken = default)
    {
        if (auth != null && !auth.EsSuperAdmin) throw new UnauthorizedAccessException("Solo superadmin puede consultar los modelos de Gemini.");
        var clave = configuracionPrivada?.Leer()?.Clave ?? configuration["Gemini:ApiKey"] ?? configuration["GEMINI_API_KEY"];
        if (string.IsNullOrWhiteSpace(clave)) throw new InvalidOperationException("Guarda una clave de Gemini antes de consultar los modelos.");
        var modelos = new List<string>();
        string? siguiente = null;
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        plazo.CancelAfter(TimeSpan.FromSeconds(30));
        do
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                "https://generativelanguage.googleapis.com/v1beta/models?pageSize=1000" +
                (siguiente == null ? "" : "&pageToken=" + Uri.EscapeDataString(siguiente)));
            request.Headers.Add("x-goog-api-key", clave);
            using var response = await httpClient.SendAsync(request, plazo.Token);
            var cuerpo = await response.Content.ReadAsStringAsync(plazo.Token);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(DescribirError(response.StatusCode, cuerpo));
            using var json = JsonDocument.Parse(cuerpo);
            if (json.RootElement.TryGetProperty("models", out var lista))
                foreach (var modelo in lista.EnumerateArray())
                    if (modelo.TryGetProperty("name", out var nombre) && nombre.GetString() is string valor && valor.StartsWith("models/gemini-", StringComparison.Ordinal) &&
                        modelo.TryGetProperty("supportedGenerationMethods", out var metodos) && metodos.EnumerateArray().Any(metodo => metodo.GetString() == "generateContent"))
                        modelos.Add(valor["models/".Length..]);
            siguiente = json.RootElement.TryGetProperty("nextPageToken", out var token) ? token.GetString() : null;
        } while (!string.IsNullOrEmpty(siguiente) && modelos.Count < 1000);
        return modelos.Distinct().OrderBy(nombre => nombre).ToList();
    }

    public async Task<string> GenerarAsync(string instrucciones, string texto, object? esquema = null,
        byte[]? documento = null, string? mime = null, CancellationToken cancellationToken = default)
    {
        if (auth != null && !auth.EstaAutenticado) throw new UnauthorizedAccessException("Inicia sesion para usar Gemini.");
        var privada = configuracionPrivada?.Leer();
        var clave = privada?.Clave ?? configuration["Gemini:ApiKey"] ?? configuration["GEMINI_API_KEY"];
        if (string.IsNullOrWhiteSpace(clave))
            throw new InvalidOperationException("Gemini no esta configurado en el servidor. Contacta con superadmin.");
        if (texto.Length > 100000 || documento?.Length > FacturaImportacionService.MaximoBytes)
            throw new ArgumentException("La solicitud supera el limite de datos permitido.");
        if (auth != null && cache != null)
        {
            lock (cache)
            {
                var key = $"gemini-cuota:{auth.EmpresaId}:{auth.UsuarioActual!.Id}";
                var cuota = cache.GetOrCreate(key, entrada =>
                {
                    entrada.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                    return new Cuota();
                })!;
                if (++cuota.Peticiones > Math.Clamp(configuration.GetValue("Gemini:RequestsPerMinute", 20), 1, 120))
                    throw new InvalidOperationException("Has alcanzado el limite de consultas de IA. Intentalo dentro de un minuto.");
            }
        }
        var entradas = new List<object>();
        if (documento != null)
        {
            if (mime is not ("application/pdf" or "image/png" or "image/jpeg" or "image/webp"))
                throw new ArgumentException("Formato de factura no compatible.");
            entradas.Add(new { inlineData = new { data = Convert.ToBase64String(documento), mimeType = mime } });
        }
        entradas.Add(new { text = texto });
        var generacion = new Dictionary<string, object> { ["maxOutputTokens"] = 4096, ["temperature"] = 0.1 };
        if (esquema != null)
        {
            generacion["responseMimeType"] = "application/json";
            generacion["responseJsonSchema"] = esquema;
        }
        var solicitud = new Dictionary<string, object>
        {
            ["contents"] = new[] { new { role = "user", parts = entradas } },
            ["systemInstruction"] = new { parts = new[] { new { text = instrucciones } } },
            ["generationConfig"] = generacion
        };
        var modelo = privada?.Modelo ?? configuration["Gemini:Model"] ?? ModeloPredeterminado;
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelo)}:generateContent");
        request.Headers.Add("x-goog-api-key", clave);
        request.Content = JsonContent.Create(solicitud);
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        plazo.CancelAfter(TimeSpan.FromSeconds(90));
        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, plazo.Token);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(DescribirError(response.StatusCode,
                    await response.Content.ReadAsStringAsync(plazo.Token)));
            var bytes = await response.Content.ReadAsByteArrayAsync(plazo.Token);
            if (bytes.Length > 1000000) throw new InvalidOperationException("La respuesta de Gemini es demasiado grande.");
            using var json = JsonDocument.Parse(bytes);
            return LeerRespuesta(json.RootElement);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("Gemini ha tardado demasiado en responder. Intentalo de nuevo.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("No se pudo conectar con Gemini.");
        }
    }

    internal static string DescribirError(HttpStatusCode codigo, string cuerpo)
    {
        var causa = string.Empty;
        if (cuerpo.Length <= 32000)
        {
            try
            {
                using var json = JsonDocument.Parse(cuerpo);
                if (json.RootElement.TryGetProperty("error", out var error))
                {
                    if (error.TryGetProperty("status", out var estado) && estado.ValueKind == JsonValueKind.String &&
                        new[] { "INVALID_ARGUMENT", "UNAUTHENTICATED", "PERMISSION_DENIED", "NOT_FOUND", "RESOURCE_EXHAUSTED", "UNAVAILABLE", "INTERNAL", "UNIMPLEMENTED" }.Contains(estado.GetString()))
                        causa = estado.GetString()!;
                    if (error.TryGetProperty("details", out var detalles) && detalles.ValueKind == JsonValueKind.Array)
                        foreach (var detalle in detalles.EnumerateArray())
                            if (detalle.TryGetProperty("reason", out var razon) && razon.ValueKind == JsonValueKind.String &&
                                new[] { "API_KEY_INVALID", "API_KEY_SERVICE_BLOCKED", "API_KEY_HTTP_REFERRER_BLOCKED", "API_KEY_IP_ADDRESS_BLOCKED", "SERVICE_DISABLED", "BILLING_DISABLED", "RATE_LIMIT_EXCEEDED", "QUOTA_EXCEEDED" }.Contains(razon.GetString()))
                                causa = razon.GetString()!;
                }
            }
            catch (JsonException) { }
        }
        var mensaje = causa switch
        {
            "API_KEY_INVALID" => "La clave de Gemini no es valida.",
            "SERVICE_DISABLED" or "API_KEY_SERVICE_BLOCKED" => "La API de Gemini no esta habilitada o permitida para esta clave.",
            "BILLING_DISABLED" => "El proyecto de Gemini requiere revisar su facturacion.",
            "API_KEY_HTTP_REFERRER_BLOCKED" or "API_KEY_IP_ADDRESS_BLOCKED" => "Las restricciones de la clave impiden utilizarla desde este servidor.",
            _ => codigo switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "Gemini rechazo la clave o sus permisos.",
                HttpStatusCode.TooManyRequests => "Se ha alcanzado la cuota o limite de Gemini.",
                HttpStatusCode.BadRequest or HttpStatusCode.NotFound => "Gemini rechazo la solicitud o el modelo configurado.",
                HttpStatusCode.NotImplemented => "El metodo de la API de Gemini no esta disponible.",
                _ => "Gemini no esta disponible en este momento."
            }
        };
        return $"{mensaje} (HTTP {(int)codigo}{(causa.Length > 0 ? ", " + causa : "")})";
    }

    internal static string LeerRespuesta(JsonElement respuesta)
    {
        if (respuesta.TryGetProperty("candidates", out var candidatos) && candidatos.ValueKind == JsonValueKind.Array && candidatos.GetArrayLength() > 0)
        {
            var candidato = candidatos[0];
            if (candidato.TryGetProperty("finishReason", out var fin) && fin.GetString() != "STOP")
                throw new InvalidOperationException("Gemini no pudo completar la respuesta. Revisa la solicitud o intentalo de nuevo.");
            if (candidato.TryGetProperty("content", out var contenido) && contenido.TryGetProperty("parts", out var partes))
            {
                var resultado = string.Join("\n", partes.EnumerateArray()
                    .Where(parte => (!parte.TryGetProperty("thought", out var pensamiento) || pensamiento.ValueKind != JsonValueKind.True) && parte.TryGetProperty("text", out _))
                    .Select(parte => parte.GetProperty("text").GetString()));
                if (!string.IsNullOrWhiteSpace(resultado)) return resultado;
            }
        }
        if (respuesta.TryGetProperty("status", out var estado) && estado.GetString() is not ("completed" or null))
            throw new InvalidOperationException("Gemini no pudo completar la respuesta.");
        if (respuesta.TryGetProperty("output_text", out var texto) && texto.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(texto.GetString()))
            return texto.GetString()!;
        if (respuesta.TryGetProperty("steps", out var pasos) && pasos.ValueKind == JsonValueKind.Array)
        {
            var textos = pasos.EnumerateArray().Where(paso => paso.TryGetProperty("content", out _))
                .SelectMany(paso => paso.GetProperty("content").EnumerateArray())
                .Where(parte => parte.TryGetProperty("type", out var tipo) && tipo.GetString() == "text" && parte.TryGetProperty("text", out _))
                .Select(parte => parte.GetProperty("text").GetString());
            var resultado = string.Join("\n", textos);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;
        }
        throw new InvalidOperationException("Gemini no ha devuelto una respuesta util.");
    }
}