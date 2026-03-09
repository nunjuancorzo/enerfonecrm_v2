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
                <div class='section-title'>👤 Datos del Interesado</div>
                {(!string.IsNullOrEmpty(oferta.NombreInteresado) ? $@"
                <div class='info-row'>
                    <span class='label'>Nombre:</span>
                    <span class='value'>{oferta.NombreInteresado}</span>
                </div>" : "")}
                {(!string.IsNullOrEmpty(oferta.TelefonoInteresado) ? $@"
                <div class='info-row'>
                    <span class='label'>Teléfono:</span>
                    <span class='value'>{oferta.TelefonoInteresado}</span>
                </div>" : "")}
                {(!string.IsNullOrEmpty(oferta.EmailInteresado) ? $@"
                <div class='info-row'>
                    <span class='label'>Email:</span>
                    <span class='value'>{oferta.EmailInteresado}</span>
                </div>" : "")}
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
                {(!string.IsNullOrEmpty(oferta.LuzGasTipoCliente) ? $@"
                <div class='info-row'>
                    <span class='label'>Tipo de Cliente:</span>
                    <span class='value'>{oferta.LuzGasTipoCliente}</span>
                </div>" : "")}
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

    public async Task<(bool exito, string mensaje)> EnviarEmailConfirmacionComercialAsync(SolicitudOferta oferta)
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
        .container {{ max-width: 700px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ background-color: #f8f9fa; padding: 20px; margin-top: 20px; border-radius: 5px; }}
        .section {{ margin-bottom: 15px; padding: 15px; background-color: white; border-left: 4px solid #28a745; }}
        .section-title {{ font-weight: bold; color: #28a745; margin-bottom: 10px; }}
        .info-row {{ margin-bottom: 8px; }}
        .label {{ font-weight: bold; }}
        .tipos-badge {{ display: inline-block; background-color: #28a745; color: white; padding: 5px 10px; border-radius: 3px; margin: 2px; }}
        .footer {{ margin-top: 20px; padding: 15px; background-color: #e9ecef; border-radius: 5px; text-align: center; font-size: 0.9rem; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>✅ Solicitud de Oferta Registrada</h2>
        </div>
        <div class='content'>
            <p>Hola <strong>{oferta.NombreComercial}</strong>,</p>
            <p>Tu solicitud de oferta ha sido registrada correctamente y será revisada por nuestro equipo.</p>
            
            <div class='section'>
                <div class='section-title'>📋 Resumen de tu Solicitud</div>
                <div class='info-row'>
                    <span class='label'>Fecha:</span> {oferta.FechaCreacion:dd/MM/yyyy HH:mm}
                </div>
                <div class='info-row'>
                    <span class='label'>Cliente:</span> {oferta.NombreInteresado}
                </div>
                <div class='info-row'>
                    <span class='label'>Teléfono:</span> {oferta.TelefonoInteresado}
                </div>
                <div class='info-row'>
                    <span class='label'>Email:</span> {oferta.EmailInteresado}
                </div>
            </div>

            <div class='section'>
                <div class='section-title'>📦 Servicios Solicitados</div>
                <div>
                    {string.Join("", tiposSeleccionados.Select(t => $"<span class='tipos-badge'>{t}</span>"))}
                </div>
            </div>

            <div class='section'>
                <div class='section-title'>⏳ Próximos Pasos</div>
                <p>Nuestro equipo revisará tu solicitud y te contactará con las mejores ofertas disponibles para tu cliente.</p>
                <p>Puedes ver el estado de esta solicitud en cualquier momento desde el panel de Ofertas en el CRM.</p>
            </div>
        </div>
        <div class='footer'>
            <p>Este es un mensaje automático. Por favor, no respondas a este correo.</p>
            <p>Si tienes alguna duda, contacta con el administrador.</p>
        </div>
    </div>
</body>
</html>";

            var resultado = await _emailService.EnviarEmailSimpleAsync(
                oferta.EmailComercial,
                $"Confirmación: Solicitud de Oferta Registrada - {oferta.NombreInteresado}",
                cuerpoHtml
            );

            return resultado;
        }
        catch (Exception ex)
        {
            return (false, $"Error al enviar email de confirmación: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> EnviarEmailCambioEstadoAsync(SolicitudOferta oferta)
    {
        try
        {
            var colorEstado = oferta.Estado switch
            {
                "Pendiente" => "#ffc107",
                "En Proceso" => "#17a2b8",
                "Completada" => "#28a745",
                _ => "#6c757d"
            };

            var iconoEstado = oferta.Estado switch
            {
                "Pendiente" => "⏳",
                "En Proceso" => "🔄",
                "Completada" => "✅",
                _ => "📋"
            };

            var cuerpoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 700px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {colorEstado}; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ background-color: #f8f9fa; padding: 20px; margin-top: 20px; border-radius: 5px; }}
        .section {{ margin-bottom: 15px; padding: 15px; background-color: white; border-left: 4px solid {colorEstado}; }}
        .section-title {{ font-weight: bold; color: {colorEstado}; margin-bottom: 10px; }}
        .estado-badge {{ display: inline-block; background-color: {colorEstado}; color: white; padding: 8px 15px; border-radius: 5px; font-weight: bold; }}
        .info-row {{ margin-bottom: 8px; }}
        .label {{ font-weight: bold; }}
        .observaciones {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin-top: 15px; border-radius: 3px; }}
        .footer {{ margin-top: 20px; padding: 15px; background-color: #e9ecef; border-radius: 5px; text-align: center; font-size: 0.9rem; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>{iconoEstado} Actualización de tu Solicitud de Oferta</h2>
        </div>
        <div class='content'>
            <p>Hola <strong>{oferta.NombreComercial}</strong>,</p>
            <p>Tu solicitud de oferta ha sido actualizada.</p>
            
            <div class='section'>
                <div class='section-title'>📋 Información de la Solicitud</div>
                <div class='info-row'>
                    <span class='label'>Solicitud #:</span> {oferta.Id}
                </div>
                <div class='info-row'>
                    <span class='label'>Cliente:</span> {oferta.NombreInteresado}
                </div>
                <div class='info-row'>
                    <span class='label'>Fecha de Creación:</span> {oferta.FechaCreacion:dd/MM/yyyy HH:mm}
                </div>
            </div>

            <div class='section'>
                <div class='section-title'>📊 Estado Actual</div>
                <div style='text-align: center; margin: 15px 0;'>
                    <span class='estado-badge'>{iconoEstado} {oferta.Estado.ToUpper()}</span>
                </div>
                {(oferta.FechaProcesado.HasValue ? $@"
                <div class='info-row' style='text-align: center; color: #666; font-size: 0.9rem;'>
                    Actualizado: {oferta.FechaProcesado.Value:dd/MM/yyyy HH:mm}
                </div>" : "")}
            </div>

            {(!string.IsNullOrEmpty(oferta.ObservacionesAdmin) ? $@"
            <div class='observaciones'>
                <div style='font-weight: bold; margin-bottom: 10px;'>💬 Comentarios del Administrador:</div>
                <div>{oferta.ObservacionesAdmin.Replace("\n", "<br/>")}</div>
            </div>" : "")}

            <div class='section'>
                <div class='section-title'>ℹ️ ¿Qué significa cada estado?</div>
                <ul style='line-height: 1.8;'>
                    <li><strong>Pendiente:</strong> Tu solicitud está en cola para ser revisada</li>
                    <li><strong>En Proceso:</strong> Estamos trabajando en conseguir las mejores ofertas</li>
                    <li><strong>Completada:</strong> Las ofertas están listas o han sido enviadas</li>
                </ul>
            </div>
        </div>
        <div class='footer'>
            <p>Puedes ver más detalles en el panel de Ofertas del CRM.</p>
            <p>Este es un mensaje automático. Por favor, no respondas a este correo.</p>
        </div>
    </div>
</body>
</html>";

            var resultado = await _emailService.EnviarEmailSimpleAsync(
                oferta.EmailComercial,
                $"Actualización: Solicitud #{oferta.Id} - {oferta.Estado} - {oferta.NombreInteresado}",
                cuerpoHtml
            );

            return resultado;
        }
        catch (Exception ex)
        {
            return (false, $"Error al enviar email de notificación: {ex.Message}");
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
