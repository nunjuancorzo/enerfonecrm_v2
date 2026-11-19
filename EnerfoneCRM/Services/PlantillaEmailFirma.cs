using System.Net;
using EnerfoneCRM.Models;

namespace EnerfoneCRM.Services;

public static class PlantillaEmailFirma
{
    public static string Crear(
        ConfiguracionEmpresa configuracion,
        string asunto,
        string nombreDestinatario,
        string descripcion,
        string textoAccion,
        string url,
        DateTime fechaCaducidadUtc)
    {
        var paleta = AparienciaService.Resolver(
            configuracion.TemaVisual,
            configuracion.TemaColorAcento,
            configuracion.TemaColorBarra,
            configuracion.TemaColorFondo);
        var accion = WebUtility.HtmlEncode(textoAccion);
        var enlace = WebUtility.HtmlEncode(url);
        var caducidad = fechaCaducidadUtc.ToLocalTime().ToString("dd/MM/yyyy");
        var colorAccion = paleta.TextoAccion;
        var contenido =
            $"<p style=\"margin:0 0 24px;font-size:16px;line-height:1.6;color:#424b55;\">{WebUtility.HtmlEncode(descripcion)}</p>" +
            $"<table role=\"presentation\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\"><tr><td align=\"center\" bgcolor=\"{paleta.Acento}\" style=\"border-radius:5px;background-color:{paleta.Acento};\">" +
            $"<a href=\"{enlace}\" style=\"display:inline-block;padding:14px 22px;border:1px solid {paleta.Acento};border-radius:5px;color:{colorAccion};font-size:15px;font-weight:bold;text-decoration:none;\">{accion}</a>" +
            "</td></tr></table>" +
            $"<p style=\"margin:24px 0 0;font-size:13px;line-height:1.5;color:#66717d;\">Este enlace es personal y estará disponible hasta el <strong>{caducidad}</strong>.</p>" +
            "<p style=\"margin:16px 0 0;font-size:13px;line-height:1.5;color:#66717d;\">Si no esperabas este mensaje, puedes ignorarlo.</p>";

        return Envolver(configuracion, asunto, nombreDestinatario, contenido);
    }

    /// <summary>
    /// Notificación sin acción: introducción, datos opcionales en formato etiqueta/valor y nota final.
    /// Usar para correos que solo informan (por ejemplo, con documentos adjuntos).
    /// </summary>
    public static string CrearNotificacion(
        ConfiguracionEmpresa configuracion,
        string asunto,
        string nombreDestinatario,
        string introduccion,
        IEnumerable<(string Etiqueta, string Valor)> datos,
        string? nota = null)
    {
        var filas = string.Concat(datos.Select(d =>
            $"<tr><td style=\"padding:6px 12px 6px 0;font-size:14px;color:#66717d;vertical-align:top;white-space:nowrap;\">{WebUtility.HtmlEncode(d.Etiqueta)}</td>" +
            $"<td style=\"padding:6px 0;font-size:14px;color:#20252b;font-weight:bold;\">{WebUtility.HtmlEncode(d.Valor)}</td></tr>"));
        var bloqueDatos = string.IsNullOrEmpty(filas)
            ? ""
            : $"<table role=\"presentation\" cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"margin:0 0 24px;\">{filas}</table>";
        var bloqueNota = string.IsNullOrWhiteSpace(nota)
            ? ""
            : $"<p style=\"margin:0 0 16px;font-size:14px;line-height:1.6;color:#424b55;\">{WebUtility.HtmlEncode(nota)}</p>";

        var contenido =
            $"<p style=\"margin:0 0 24px;font-size:16px;line-height:1.6;color:#424b55;\">{WebUtility.HtmlEncode(introduccion)}</p>" +
            bloqueDatos + bloqueNota;

        return Envolver(configuracion, asunto, nombreDestinatario, contenido);
    }

    private static string Envolver(ConfiguracionEmpresa configuracion, string asunto, string nombreDestinatario, string contenidoHtml)
    {
        var paleta = AparienciaService.Resolver(
            configuracion.TemaVisual,
            configuracion.TemaColorAcento,
            configuracion.TemaColorBarra,
            configuracion.TemaColorFondo);
        var empresa = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(configuracion.NombreEmpresa)
            ? "Voltia Solutions"
            : configuracion.NombreEmpresa.Trim());
        var logoHtml = $"<span style=\"color:{paleta.TextoBarra};font-size:22px;font-weight:bold;\">{empresa}</span>";
        var nombre = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(nombreDestinatario) ? "" : nombreDestinatario.Trim());
        var titulo = WebUtility.HtmlEncode(asunto);
        var contacto = WebUtility.HtmlEncode(configuracion.Email ?? configuracion.Web ?? "");
        var fondo = paleta.Fondo;
        var barra = paleta.BarraLateral;
        var acento = paleta.Acento;

        return $"""
            <!doctype html>
            <html lang="es">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background-color:{fondo};font-family:Arial,Helvetica,sans-serif;color:#20252b;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:{fondo};width:100%;">
                <tr><td align="center" style="padding:32px 12px;">
                  <table role="presentation" width="600" cellspacing="0" cellpadding="0" border="0" style="width:100%;max-width:600px;background-color:#ffffff;border:1px solid #e3e7eb;border-radius:8px;overflow:hidden;">
                    <tr><td align="center" bgcolor="#ffffff" style="background-color:#ffffff;padding:24px 32px;text-align:center;">{logoHtml}</td></tr>
                    <tr><td style="height:5px;line-height:5px;background-color:{acento};font-size:0;">&nbsp;</td></tr>
                    <tr><td style="padding:32px;">
                      <p style="margin:0 0 8px;color:#66717d;font-size:14px;">Hola {nombre},</p>
                      <h1 style="margin:0 0 16px;font-size:24px;line-height:1.3;color:#20252b;">{titulo}</h1>
                      {contenidoHtml}
                    </td></tr>
                    <tr><td style="padding:18px 32px;background-color:#f7f8fa;border-top:1px solid #e3e7eb;font-size:12px;line-height:1.5;color:#66717d;">
                      <strong style="color:{barra};">{empresa}</strong>{(string.IsNullOrEmpty(contacto) ? "" : $"<br>{contacto}")}
                    </td></tr>
                  </table>
                  <p style="margin:14px 0 0;font-size:11px;color:#77818b;">Mensaje automático. No respondas a este correo.</p>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }
}