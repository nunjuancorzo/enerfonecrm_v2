using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class OfertaService
{
    private readonly DbContextProvider _dbContextProvider;
    private readonly EmailService _emailService;

    public OfertaService(DbContextProvider dbContextProvider, EmailService emailService)
    {
        _dbContextProvider = dbContextProvider;
        _emailService = emailService;
    }

    public async Task<List<SolicitudOferta>> ObtenerTodasAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.SolicitudesOfertas
            .OrderByDescending(o => o.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<SolicitudOferta>> ObtenerPorUsuarioAsync(int usuarioId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.SolicitudesOfertas
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.FechaCreacion)
            .ToListAsync();
    }

    public async Task<SolicitudOferta?> ObtenerPorIdAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.SolicitudesOfertas.FindAsync(id);
    }

    public async Task<bool> CrearAsync(SolicitudOferta oferta)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            context.SolicitudesOfertas.Add(oferta);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ActualizarAsync(SolicitudOferta oferta)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            context.SolicitudesOfertas.Update(oferta);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EliminarAsync(int id)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var oferta = await context.SolicitudesOfertas.FindAsync(id);
            if (oferta == null) return false;

            context.SolicitudesOfertas.Remove(oferta);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool exito, string mensaje)> EnviarEmailSolicitudAsync(SolicitudOferta oferta, string emailAdministrador)
    {
        try
        {
            var tiposSeleccionados = new List<string>();
            if (oferta.TipoLuz) tiposSeleccionados.Add("Luz");
            if (oferta.TipoGas) tiposSeleccionados.Add("Gas");
            if (oferta.TipoFotovoltaica) tiposSeleccionados.Add("Fotovoltaica");
            if (oferta.TipoFibra) tiposSeleccionados.Add("Fibra");
            if (oferta.TipoMovil) tiposSeleccionados.Add("Móvil");
            if (oferta.TipoFibraMovil) tiposSeleccionados.Add("Fibra + Móvil");
            if (oferta.TipoFibraMovilTv) tiposSeleccionados.Add("Fibra + Móvil + TV");
            if (oferta.TipoAlarma) tiposSeleccionados.Add("Alarma");

            var cuerpoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 800px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; border-radius: 5px; }}
        .content {{ background-color: #f8f9fa; padding: 20px; margin-top: 20px; border-radius: 5px; }}
        .section {{ margin-bottom: 20px; padding: 15px; background-color: white; border-left: 4px solid #007bff; }}
        .section-title {{ font-weight: bold; color: #007bff; margin-bottom: 10px; }}
        .info-row {{ margin-bottom: 10px; }}
        .label {{ font-weight: bold; display: inline-block; width: 200px; }}
        .value {{ color: #555; }}
        .tipos-badge {{ display: inline-block; background-color: #28a745; color: white; padding: 5px 10px; border-radius: 3px; margin: 2px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🔔 Nueva Solicitud de Oferta</h2>
        </div>
        <div class='content'>
            <div class='section'>
                <div class='section-title'>Información del Comercial</div>
                <div class='info-row'>
                    <span class='label'>Comercial:</span>
                    <span class='value'>{oferta.NombreComercial}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Email:</span>
                    <span class='value'>{oferta.EmailComercial}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Fecha de Solicitud:</span>
                    <span class='value'>{oferta.FechaCreacion:dd/MM/yyyy HH:mm}</span>
                </div>
            </div>

            <div class='section'>
                <div class='section-title'>Tipos de Oferta Solicitados</div>
                <div>
                    {string.Join("", tiposSeleccionados.Select(t => $"<span class='tipos-badge'>{t}</span>"))}
                </div>
            </div>";

            // Luz/Gas
            if (oferta.TipoLuz || oferta.TipoGas)
            {
                cuerpoHtml += $@"
            <div class='section'>
                <div class='section-title'>📊 Luz / Gas</div>
                <div class='info-row'>
                    <span class='label'>Factura adjunta:</span>
                    <span class='value'>{(string.IsNullOrEmpty(oferta.LuzGasRutaFactura) ? "No" : "Sí")}</span>
                </div>
                {(!string.IsNullOrEmpty(oferta.LuzGasObservaciones) ? $@"
                <div class='info-row'>
                    <span class='label'>Observaciones:</span>
                    <div class='value'>{oferta.LuzGasObservaciones}</div>
                </div>" : "")}
            </div>";
            }

            // Fotovoltaica
            if (oferta.TipoFotovoltaica)
            {
                cuerpoHtml += $@"
            <div class='section'>
                <div class='section-title'>☀️ Fotovoltaica</div>
                <div class='info-row'>
                    <span class='label'>Factura adjunta:</span>
                    <span class='value'>{(string.IsNullOrEmpty(oferta.FotovoltaicaRutaFactura) ? "No" : "Sí")}</span>
                </div>
                {(!string.IsNullOrEmpty(oferta.FotovoltaicaEnlaceMaps) ? $@"
                <div class='info-row'>
                    <span class='label'>Ubicación Google Maps:</span>
                    <div class='value'><a href='{oferta.FotovoltaicaEnlaceMaps}' target='_blank'>{oferta.FotovoltaicaEnlaceMaps}</a></div>
                </div>" : "")}
                {(!string.IsNullOrEmpty(oferta.FotovoltaicaObservaciones) ? $@"
                <div class='info-row'>
                    <span class='label'>Observaciones:</span>
                    <div class='value'>{oferta.FotovoltaicaObservaciones}</div>
                </div>" : "")}
            </div>";
            }

            // Telefonía
            if (oferta.TipoFibra || oferta.TipoMovil || oferta.TipoFibraMovil || oferta.TipoFibraMovilTv)
            {
                cuerpoHtml += $@"
            <div class='section'>
                <div class='section-title'>📱 Telefonía</div>
                {(!string.IsNullOrEmpty(oferta.TelefoniaTipoSolicitud) ? $@"
                <div class='info-row'>
                    <span class='label'>Tipo de solicitud:</span>
                    <span class='value'>{oferta.TelefoniaTipoSolicitud}</span>
                </div>" : "")}
                <div class='info-row'>
                    <span class='label'>Factura adjunta:</span>
                    <span class='value'>{(string.IsNullOrEmpty(oferta.TelefoniaRutaFactura) ? "No" : "Sí")}</span>
                </div>
                {(!string.IsNullOrEmpty(oferta.TelefoniaContratoActual) ? $@"
                <div class='info-row'>
                    <span class='label'>Contrato actual:</span>
                    <div class='value'>{oferta.TelefoniaContratoActual}</div>
                </div>" : "")}
                {(!string.IsNullOrEmpty(oferta.TelefoniaObservaciones) ? $@"
                <div class='info-row'>
                    <span class='label'>Observaciones:</span>
                    <div class='value'>{oferta.TelefoniaObservaciones}</div>
                </div>" : "")}
            </div>";
            }

            // Alarma
            if (oferta.TipoAlarma)
            {
                cuerpoHtml += $@"
            <div class='section'>
                <div class='section-title'>🚨 Alarma</div>
                {(!string.IsNullOrEmpty(oferta.AlarmaTipo) ? $@"
                <div class='info-row'>
                    <span class='label'>Tipo:</span>
                    <span class='value'>{oferta.AlarmaTipo}</span>
                </div>" : "")}
                {(oferta.AlarmaTieneActual.HasValue ? $@"
                <div class='info-row'>
                    <span class='label'>Tiene alarma actualmente:</span>
                    <span class='value'>{(oferta.AlarmaTieneActual.Value ? "Sí" : "No")}</span>
                </div>" : "")}
                {(!string.IsNullOrEmpty(oferta.AlarmaObservaciones) ? $@"
                <div class='info-row'>
                    <span class='label'>Observaciones:</span>
                    <div class='value'>{oferta.AlarmaObservaciones}</div>
                </div>" : "")}
            </div>";
            }

            cuerpoHtml += @"
        </div>
        <div style='margin-top: 20px; padding: 15px; background-color: #f8f9fa; border-radius: 5px; text-align: center;'>
            <p>Para gestionar esta solicitud, accede al panel de administración del CRM</p>
        </div>
    </div>
</body>
</html>";

            // Recopilar archivos adjuntos
            var archivosAdjuntos = new List<string>();
            if (!string.IsNullOrEmpty(oferta.LuzGasRutaFactura) && File.Exists(oferta.LuzGasRutaFactura))
            {
                archivosAdjuntos.Add(oferta.LuzGasRutaFactura);
            }
            if (!string.IsNullOrEmpty(oferta.FotovoltaicaRutaFactura) && File.Exists(oferta.FotovoltaicaRutaFactura))
            {
                archivosAdjuntos.Add(oferta.FotovoltaicaRutaFactura);
            }
            if (!string.IsNullOrEmpty(oferta.TelefoniaRutaFactura) && File.Exists(oferta.TelefoniaRutaFactura))
            {
                archivosAdjuntos.Add(oferta.TelefoniaRutaFactura);
            }

            // Enviar email con adjuntos
            var resultado = await _emailService.EnviarEmailConAdjuntosMultiplesAsync(
                emailAdministrador,
                $"Nueva Solicitud de Oferta - {oferta.NombreComercial}",
                cuerpoHtml,
                archivosAdjuntos
            );

            return resultado;
        }
        catch (Exception ex)
        {
            return (false, $"Error al enviar email: {ex.Message}");
        }
    }

    public async Task<(bool exito, byte[]? contenido, string? nombreArchivo, string? mimeType)> DescargarArchivoAsync(string rutaArchivo)
    {
        try
        {
            if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
            {
                return (false, null, null, null);
            }

            var contenido = await File.ReadAllBytesAsync(rutaArchivo);
            var nombreArchivo = Path.GetFileName(rutaArchivo);
            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
            
            var mimeType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            return (true, contenido, nombreArchivo, mimeType);
        }
        catch
        {
            return (false, null, null, null);
        }
    }
}
