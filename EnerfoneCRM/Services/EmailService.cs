using System.Net;
using System.Net.Mail;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class EmailService
{
    private readonly DbContextProvider _dbContextProvider;

    public EmailService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public static bool EsEmailValido(string? email) => !string.IsNullOrWhiteSpace(email) &&
        new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email.Trim());

    public static string ObtenerIdentificacionCliente(Cliente cliente)
    {
        var (nombre, representante, esPyme) = ObtenerDatosCliente(cliente);
        return esPyme && !string.IsNullOrWhiteSpace(representante)
            ? $"{nombre} - Representante: {representante}" : nombre;
    }

    public static string GenerarDatosClienteNotificacion(Cliente cliente)
    {
        var (nombre, representante, esPyme) = ObtenerDatosCliente(cliente);
        var etiqueta = esPyme ? "Pyme / Razón social" : "Cliente";
        var html = $"<p><span class='label'>{etiqueta}:</span> <span class='value'>{WebUtility.HtmlEncode(nombre)}</span></p>";
        if (esPyme)
            html += $"<p><span class='label'>Representante:</span> <span class='value'>{WebUtility.HtmlEncode(representante ?? "No informado")}</span></p>";
        return html;
    }

    private static (string nombre, string? representante, bool esPyme) ObtenerDatosCliente(Cliente cliente)
    {
        var esPyme = string.Equals(cliente.TipoCliente?.Trim(), "Pyme", StringComparison.OrdinalIgnoreCase);
        var nombre = esPyme && !string.IsNullOrWhiteSpace(cliente.Empresa) ? cliente.Empresa.Trim() : cliente.Nombre?.Trim();
        var representante = cliente.Representante?.Trim();
        if (esPyme && string.IsNullOrWhiteSpace(representante) && !string.IsNullOrWhiteSpace(cliente.Empresa) &&
            !string.Equals(cliente.Nombre?.Trim(), nombre, StringComparison.OrdinalIgnoreCase))
            representante = cliente.Nombre?.Trim();
        return (string.IsNullOrWhiteSpace(nombre) ? "No informado" : nombre,
            string.IsNullOrWhiteSpace(representante) ? null : representante, esPyme);
    }

    public async Task<(bool exito, string mensaje)> EnviarEmailConAdjuntoAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml,
        byte[] adjuntoPdf,
        string nombreAdjunto,
        string tipoMime = "application/pdf")
    {
        try
        {
            // Obtener configuración SMTP
            using var context = _dbContextProvider.CreateDbContext();
            var config = await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();

            if (config == null)
            {
                return (false, "No se encontró la configuración de la empresa");
            }

            if (string.IsNullOrEmpty(config.SmtpServidor) || 
                string.IsNullOrEmpty(config.SmtpUsuario) || 
                string.IsNullOrEmpty(config.SmtpPassword))
            {
                return (false, "La configuración SMTP está incompleta. Configure el servidor de email en Configuración de Empresa");
            }

            // Crear el mensaje
            using var message = new MailMessage();
            message.From = new MailAddress(
                config.SmtpEmailDesde ?? config.SmtpUsuario,
                config.SmtpNombreDesde ?? config.NombreEmpresa
            );
            message.To.Add(destinatario);
            message.Subject = asunto;
            message.Body = cuerpoHtml;
            message.IsBodyHtml = true;

            // Adjuntar el PDF
            if (adjuntoPdf != null && adjuntoPdf.Length > 0)
            {
                var stream = new MemoryStream(adjuntoPdf);
                var attachment = new Attachment(stream, nombreAdjunto, tipoMime);
                message.Attachments.Add(attachment);
            }

            // Configurar el cliente SMTP
            using var smtp = new SmtpClient(config.SmtpServidor, config.SmtpPuerto ?? 587);
            smtp.Credentials = new NetworkCredential(config.SmtpUsuario, config.SmtpPassword);
            smtp.EnableSsl = config.SmtpUsarSsl;

            // Enviar el email
            await smtp.SendMailAsync(message);

            return (true, "Email enviado correctamente");
        }
        catch (SmtpException smtpEx)
        {
            return (false, $"Error al enviar email: {smtpEx.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error inesperado al enviar email: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> EnviarEmailSimpleAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml)
    {
        return await EnviarEmailConAdjuntoAsync(destinatario, asunto, cuerpoHtml, Array.Empty<byte>(), string.Empty);
    }

    public async Task<(bool exito, string mensaje)> EnviarEmailConAdjuntosMultiplesAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml,
        List<string> rutasArchivos)
    {
        try
        {
            // Obtener configuración SMTP
            using var context = _dbContextProvider.CreateDbContext();
            var config = await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();

            if (config == null)
            {
                return (false, "No se encontró la configuración de la empresa");
            }

            if (string.IsNullOrEmpty(config.SmtpServidor) || 
                string.IsNullOrEmpty(config.SmtpUsuario) || 
                string.IsNullOrEmpty(config.SmtpPassword))
            {
                return (false, "La configuración SMTP está incompleta. Configure el servidor de email en Configuración de Empresa");
            }

            // Crear el mensaje
            using var message = new MailMessage();
            message.From = new MailAddress(
                config.SmtpEmailDesde ?? config.SmtpUsuario,
                config.SmtpNombreDesde ?? config.NombreEmpresa
            );
            message.To.Add(destinatario);
            message.Subject = asunto;
            message.Body = cuerpoHtml;
            message.IsBodyHtml = true;

            // Adjuntar múltiples archivos
            if (rutasArchivos != null && rutasArchivos.Any())
            {
                foreach (var rutaArchivo in rutasArchivos)
                {
                    if (File.Exists(rutaArchivo))
                    {
                        var nombreArchivo = Path.GetFileName(rutaArchivo);
                        var bytes = await File.ReadAllBytesAsync(rutaArchivo);
                        var stream = new MemoryStream(bytes);
                        var mimeType = ObtenerMimeType(nombreArchivo);
                        var attachment = new Attachment(stream, nombreArchivo, mimeType);
                        message.Attachments.Add(attachment);
                    }
                }
            }

            // Configurar el cliente SMTP
            using var smtp = new SmtpClient(config.SmtpServidor, config.SmtpPuerto ?? 587);
            smtp.Credentials = new NetworkCredential(config.SmtpUsuario, config.SmtpPassword);
            smtp.EnableSsl = config.SmtpUsarSsl;

            // Enviar el email
            await smtp.SendMailAsync(message);

            return (true, "Email enviado correctamente");
        }
        catch (SmtpException smtpEx)
        {
            return (false, $"Error al enviar email: {smtpEx.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error inesperado al enviar email: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> EnviarEmailConAdjuntosEnMemoriaAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml,
        IReadOnlyCollection<(byte[] Contenido, string NombreArchivo, string TipoMime)> adjuntos)
    {
        try
        {
            using var context = _dbContextProvider.CreateDbContext();
            var config = await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();

            if (config == null)
                return (false, "No se encontró la configuración de la empresa");

            if (string.IsNullOrEmpty(config.SmtpServidor) ||
                string.IsNullOrEmpty(config.SmtpUsuario) ||
                string.IsNullOrEmpty(config.SmtpPassword))
                return (false, "La configuración SMTP está incompleta. Configure el servidor de email en Configuración de Empresa");

            using var message = new MailMessage
            {
                From = new MailAddress(config.SmtpEmailDesde ?? config.SmtpUsuario, config.SmtpNombreDesde ?? config.NombreEmpresa),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };
            message.To.Add(destinatario);

            foreach (var adjunto in adjuntos)
            {
                var stream = new MemoryStream(adjunto.Contenido);
                message.Attachments.Add(new Attachment(stream, adjunto.NombreArchivo, adjunto.TipoMime));
            }

            using var smtp = new SmtpClient(config.SmtpServidor, config.SmtpPuerto ?? 587)
            {
                Credentials = new NetworkCredential(config.SmtpUsuario, config.SmtpPassword),
                EnableSsl = config.SmtpUsarSsl
            };
            await smtp.SendMailAsync(message);

            return (true, "Email enviado correctamente");
        }
        catch (SmtpException smtpEx)
        {
            return (false, $"Error al enviar email: {smtpEx.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error inesperado al enviar email: {ex.Message}");
        }
    }

    private string ObtenerMimeType(string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
