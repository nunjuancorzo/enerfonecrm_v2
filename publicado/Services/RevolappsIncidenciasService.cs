using System.Net.Http.Headers;
using System.Text.Json;
using EnerfoneCRM.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace EnerfoneCRM.Services;

public class RevolappsIncidenciasService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RevolappsIncidenciasService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<(bool exito, string mensaje, int? incidentId)> EnviarIncidenciaAsync(Incidencia incidencia, CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["IncidenciasRevolapps:BaseUrl"];
        var apiKey = _configuration["IncidenciasRevolapps:ApiKey"];
        var appKey = _configuration["IncidenciasRevolapps:AppKey"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return (false, "Falta configurar IncidenciasRevolapps:BaseUrl en appsettings", null);
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return (false, "Falta configurar IncidenciasRevolapps:ApiKey en appsettings", null);
        }

        if (string.IsNullOrWhiteSpace(appKey))
        {
            return (false, "Falta configurar IncidenciasRevolapps:AppKey en appsettings", null);
        }

        var endpoint = new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), "api/incidents");

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(appKey), "app_key");
        form.Add(new StringContent(incidencia.Asunto ?? string.Empty), "subject");
        form.Add(new StringContent(MapearTipoIncidencia(incidencia.TipoIncidencia)), "incident_type");
        form.Add(new StringContent(MapearPrioridad(incidencia.Prioridad)), "priority");

        var descripcion = (incidencia.Descripcion ?? string.Empty)
            + $"\n\n[Origen] EnerfoneCRM\n[IncidenciaId] {incidencia.Id}\n[FechaCreacion] {incidencia.FechaCreacion:yyyy-MM-dd HH:mm:ss}";
        form.Add(new StringContent(descripcion), "description");

        if (!string.IsNullOrWhiteSpace(incidencia.NombreUsuario))
        {
            form.Add(new StringContent(incidencia.NombreUsuario), "user_name");
        }

        if (!string.IsNullOrWhiteSpace(incidencia.EmailUsuario))
        {
            form.Add(new StringContent(incidencia.EmailUsuario), "user_email");
        }

        // Adjunto opcional si está disponible en almacenamiento
        if (incidencia.TieneImagen && !string.IsNullOrWhiteSpace(incidencia.NombreImagen))
        {
            var rutaAdjunto = ObtenerRutaImagenIncidencia(incidencia.NombreImagen);
            if (File.Exists(rutaAdjunto))
            {
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(rutaAdjunto, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var stream = File.OpenRead(rutaAdjunto);
                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                // El nombre de fichero que viaja al servidor
                form.Add(content, "attachment", Path.GetFileName(rutaAdjunto));
            }
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("x-api-key", apiKey);
        request.Content = form;

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, $"Error de red enviando incidencia: {ex.Message}", null);
        }

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return (false, $"Error creando incidencia remota: {(int)response.StatusCode} {response.ReasonPhrase}. {responseText}", null);
        }

        try
        {
            using var doc = JsonDocument.Parse(responseText);
            if (doc.RootElement.TryGetProperty("incident", out var incidentElem) &&
                incidentElem.TryGetProperty("id", out var idElem) &&
                idElem.TryGetInt32(out var id))
            {
                return (true, "Incidencia enviada correctamente", id);
            }
        }
        catch
        {
            // Ignorar parseo; se considera éxito si HTTP es ok
        }

        return (true, "Incidencia enviada correctamente", null);
    }

    private string MapearPrioridad(string prioridad)
    {
        return prioridad?.Trim() switch
        {
            "Crítica" => "critica",
            "Alta" => "alta",
            "Media" => "media",
            "Baja" => "baja",
            _ => "media"
        };
    }

    private string MapearTipoIncidencia(string tipo)
    {
        return tipo?.Trim() switch
        {
            "Error Técnico" => "bug",
            "Problema de Datos" => "soporte",
            "Solicitud de Mejora" => "mejora",
            "Consulta" => "soporte",
            "Otro" => "otro",
            _ => "otro"
        };
    }

    private string ObtenerRutaImagenIncidencia(string nombreImagen)
    {
        var storagePath = _configuration.GetValue<string>("StoragePath");
        if (string.IsNullOrEmpty(storagePath))
        {
            storagePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
        }

        var safeName = Path.GetFileName(nombreImagen);
        return Path.Combine(storagePath, "incidencias", safeName);
    }
}
