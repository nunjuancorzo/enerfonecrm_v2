using System.Net;
using EnerfoneCRM.Models;

namespace EnerfoneCRM.Services;

public sealed record LogoInlineEmail(byte[] Contenido, string TipoMime, string IdContenido);
public sealed record CorreoFirmaHtml(string Html, LogoInlineEmail? Logo);

public static class PlantillaEmailFirma
{
    private const string IdLogo = "logo-empresa";
    private const int MaximoLogoBytes = 2 * 1024 * 1024;

    public static CorreoFirmaHtml Crear(
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
        var logo = LeerLogo(configuracion.LogoUrl);
        var empresa = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(configuracion.NombreEmpresa)
            ? "Enerfone CRM"
            : configuracion.NombreEmpresa.Trim());
        var logoHtml = logo != null
          ? $"<img src=\"cid:{IdLogo}\" alt=\"{empresa}\" width=\"180\" style=\"display:block;width:180px;max-width:100%;height:auto;margin:0 auto;border:0;\">"
            : $"<span style=\"color:{paleta.TextoBarra};font-size:22px;font-weight:bold;\">{empresa}</span>";
        var nombre = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(nombreDestinatario) ? "" : nombreDestinatario.Trim());
        var texto = WebUtility.HtmlEncode(descripcion);
        var titulo = WebUtility.HtmlEncode(asunto);
        var accion = WebUtility.HtmlEncode(textoAccion);
        var enlace = WebUtility.HtmlEncode(url);
        var caducidad = fechaCaducidadUtc.ToLocalTime().ToString("dd/MM/yyyy");
        var contacto = WebUtility.HtmlEncode(configuracion.Email ?? configuracion.Web ?? "");
        var fondo = paleta.Fondo;
        var barra = paleta.BarraLateral;
        var acento = paleta.Acento;
        var colorAccion = paleta.TextoAccion;
        var colorBarra = paleta.TextoBarra;

        var html = $"""
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
                      <p style="margin:0 0 24px;font-size:16px;line-height:1.6;color:#424b55;">{texto}</p>
                      <table role="presentation" cellspacing="0" cellpadding="0" border="0"><tr><td align="center" bgcolor="{acento}" style="border-radius:5px;background-color:{acento};">
                        <a href="{enlace}" style="display:inline-block;padding:14px 22px;border:1px solid {acento};border-radius:5px;color:{colorAccion};font-size:15px;font-weight:bold;text-decoration:none;">{accion}</a>
                      </td></tr></table>
                      <p style="margin:24px 0 0;font-size:13px;line-height:1.5;color:#66717d;">Este enlace es personal y estará disponible hasta el <strong>{caducidad}</strong>.</p>
                      <p style="margin:16px 0 0;font-size:13px;line-height:1.5;color:#66717d;">Si no esperabas este mensaje, puedes ignorarlo.</p>
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

        return new(html, logo);
    }

    private static LogoInlineEmail? LeerLogo(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl) || !logoUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return null;

        var separador = logoUrl.IndexOf(',');
        if (separador < 0 || !logoUrl[..separador].EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
            return null;

        var tipoMime = logoUrl[5..logoUrl.IndexOf(';')].ToLowerInvariant();
        if (tipoMime is not ("image/png" or "image/jpeg" or "image/gif" or "image/webp"))
            return null;

        try
        {
            var contenido = Convert.FromBase64String(logoUrl[(separador + 1)..]);
            return contenido.Length is > 0 and <= MaximoLogoBytes
                ? new LogoInlineEmail(contenido, tipoMime, IdLogo)
                : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}