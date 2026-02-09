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
}
