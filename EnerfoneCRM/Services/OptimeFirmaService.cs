using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace EnerfoneCRM.Services;

public class OptimeFirmaService
{
    private const int DiasCaducidad = 7;
    private static readonly CultureInfo Es = new("es-ES");

    private readonly DbContextProvider _dbContextProvider;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<OptimeFirmaService> _logger;
    private readonly FirmaTokenService _tokenService = new();

    public OptimeFirmaService(
        DbContextProvider dbContextProvider,
        EmailService emailService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OptimeFirmaService> logger)
    {
        _dbContextProvider = dbContextProvider;
        _emailService = emailService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>Envía los tres documentos de alta (un email por documento).</summary>
    public async Task<ResultadoFirma> EnviarDocumentosAsync(int solicitudId, string? baseUrlAlternativa)
    {
        var errores = new List<string>();
        foreach (var tipo in OptimeFirma.Tipos)
        {
            var resultado = await EnviarDocumentoAsync(solicitudId, tipo, baseUrlAlternativa);
            if (!resultado.Exito) errores.Add($"{OptimeFirma.Titulo(tipo)}: {resultado.Mensaje}");
        }

        return errores.Count == 0
            ? new(true, "Documentos enviados a firma.")
            : new(false, string.Join(" | ", errores));
    }

    public async Task<ResultadoFirma> EnviarDocumentoAsync(int solicitudId, string tipo, string? baseUrlAlternativa)
    {
        if (!OptimeFirma.Tipos.Contains(tipo)) return new(false, "Tipo de documento no válido.");

        await using var context = _dbContextProvider.CreateDbContext();
        var solicitud = await context.OptimeSolicitudes.AsNoTracking().FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud == null) return new(false, "Solicitud no encontrada.");
        if (string.IsNullOrWhiteSpace(solicitud.Email) || !new EmailAddressAttribute().IsValid(solicitud.Email))
            return new(false, "La solicitud no tiene un email válido.");

        byte[] original;
        try
        {
            original = GenerarDocumento(tipo, solicitud, null, null, DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando documento Optime {Tipo} para solicitud {Id}", tipo, solicitudId);
            return new(false, "No se pudo generar el documento.");
        }

        var anteriores = await context.OptimeFirmas
            .Where(f => f.SolicitudId == solicitudId && f.TipoDocumento == tipo && f.Estado != EstadoSolicitudFirma.Firmado && f.Estado != EstadoSolicitudFirma.Cancelado)
            .ToListAsync();
        foreach (var anterior in anteriores) anterior.Estado = EstadoSolicitudFirma.Cancelado;

        var ahora = DateTime.UtcNow;
        var token = _tokenService.Generar();
        var firma = new OptimeFirma
        {
            SolicitudId = solicitudId,
            TipoDocumento = tipo,
            ProcesoId = Guid.NewGuid().ToString("D"),
            TokenHash = _tokenService.Hash(token),
            FechaCreacionUtc = ahora,
            FechaCaducidadUtc = ahora.AddDays(DiasCaducidad),
            Estado = EstadoSolicitudFirma.Enviado,
            EmailDestinatario = solicitud.Email.Trim(),
            NombreDestinatario = solicitud.PersonaContacto,
            DocumentoOriginal = original,
            HashDocumentoOriginal = ContractSigningPdfService.CalcularHash(original)
        };
        context.OptimeFirmas.Add(firma);
        await context.SaveChangesAsync();

        var url = $"{ObtenerBaseUrl(baseUrlAlternativa)}/firma-optime/{Uri.EscapeDataString(token)}";
        var titulo = OptimeFirma.Titulo(tipo);
        var cuerpo =
            $"<p>Hola {WebUtility.HtmlEncode(solicitud.PersonaContacto)},</p>" +
            $"<p>Tiene disponible para su firma el documento <strong>{WebUtility.HtmlEncode(titulo)}</strong> correspondiente al alta del punto de venta <strong>{WebUtility.HtmlEncode(solicitud.RazonSocial)}</strong>.</p>" +
            $"<p><a href=\"{WebUtility.HtmlEncode(url)}\">Consultar y firmar documento</a></p>" +
            $"<p>Este enlace es personal y válido hasta el {firma.FechaCaducidadUtc.ToLocalTime():dd/MM/yyyy}.</p>";

        var email = await _emailService.EnviarEmailSimpleAsync(firma.EmailDestinatario, $"{titulo} - pendiente de firma", cuerpo);
        if (!email.exito)
        {
            firma.ErrorEnvio = email.mensaje.Length > 500 ? email.mensaje[..500] : email.mensaje;
            await context.SaveChangesAsync();
            return new(false, $"No se pudo enviar el email: {email.mensaje}", url);
        }

        return new(true, "Documento enviado a firma.", url);
    }

    public async Task<List<OptimeFirma>> ObtenerUltimasFirmasAsync(int solicitudId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var firmas = await context.OptimeFirmas.AsNoTracking()
            .Where(f => f.SolicitudId == solicitudId)
            .Select(f => new OptimeFirma
            {
                Id = f.Id,
                SolicitudId = f.SolicitudId,
                TipoDocumento = f.TipoDocumento,
                FechaCreacionUtc = f.FechaCreacionUtc,
                FechaCaducidadUtc = f.FechaCaducidadUtc,
                FechaFirmaUtc = f.FechaFirmaUtc,
                Estado = f.Estado,
                EmailDestinatario = f.EmailDestinatario,
                ErrorEnvio = f.ErrorEnvio,
                IpFirma = f.IpFirma,
                HashDocumentoFirmado = f.HashDocumentoFirmado
            })
            .ToListAsync();

        foreach (var f in firmas.Where(f => f.Estado != EstadoSolicitudFirma.Firmado && f.Estado != EstadoSolicitudFirma.Cancelado && f.FechaCaducidadUtc <= DateTime.UtcNow))
            f.Estado = EstadoSolicitudFirma.Caducado;

        return firmas
            .GroupBy(f => f.TipoDocumento)
            .Select(g => g.OrderByDescending(f => f.FechaCreacionUtc).First())
            .ToList();
    }

    public async Task<(byte[]? Contenido, string? Nombre)> ObtenerDocumentoPrivadoAsync(int firmaId, bool firmado)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var firma = await context.OptimeFirmas.AsNoTracking().FirstOrDefaultAsync(f => f.Id == firmaId);
        if (firma == null) return (null, null);
        var contenido = firmado ? firma.DocumentoFirmado : firma.DocumentoOriginal;
        return contenido == null ? (null, null) : (contenido, NombreArchivo(firma.TipoDocumento, firmado));
    }

    public async Task<OptimeFirma?> ObtenerPorTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 256) return null;

        await using var context = _dbContextProvider.CreateDbContext();
        var hash = _tokenService.Hash(token);
        var firma = await context.OptimeFirmas.FirstOrDefaultAsync(f => f.TokenHash == hash);
        if (firma == null || firma.Estado is EstadoSolicitudFirma.Cancelado or EstadoSolicitudFirma.Caducado) return null;

        if (firma.Estado != EstadoSolicitudFirma.Firmado && firma.FechaCaducidadUtc <= DateTime.UtcNow)
        {
            firma.Estado = EstadoSolicitudFirma.Caducado;
            await context.SaveChangesAsync();
            return null;
        }

        if (firma.Estado == EstadoSolicitudFirma.Enviado)
        {
            firma.Estado = EstadoSolicitudFirma.EnProceso;
            firma.IpFirma ??= ObtenerIp();
            await context.SaveChangesAsync();
        }

        return firma;
    }

    public async Task<ResultadoFirma> CompletarFirmaAsync(string token, byte[] imagenFirma)
    {
        if (imagenFirma.Length == 0 || imagenFirma.Length > 2 * 1024 * 1024)
            return new(false, "La firma no es válida.");
        if (string.IsNullOrWhiteSpace(token) || token.Length > 256)
            return new(false, "El enlace no es válido o ha caducado.");

        await using var context = _dbContextProvider.CreateDbContext();
        var hash = _tokenService.Hash(token);
        var firma = await context.OptimeFirmas.FirstOrDefaultAsync(f => f.TokenHash == hash);
        if (firma == null || firma.Estado is not (EstadoSolicitudFirma.Enviado or EstadoSolicitudFirma.EnProceso) || firma.FechaCaducidadUtc <= DateTime.UtcNow)
            return new(false, "El enlace no es válido o ha caducado.");

        var solicitud = await context.OptimeSolicitudes.AsNoTracking().FirstOrDefaultAsync(s => s.Id == firma.SolicitudId);
        if (solicitud == null) return new(false, "Solicitud no encontrada.");

        var ip = string.IsNullOrWhiteSpace(firma.IpFirma) ? ObtenerIp() : firma.IpFirma;
        var ahora = DateTime.Now;
        var firmado = GenerarDocumento(firma.TipoDocumento, solicitud, imagenFirma, ip, ahora);

        firma.DocumentoFirmado = firmado;
        firma.HashDocumentoFirmado = ContractSigningPdfService.CalcularHash(firmado);
        firma.FirmaImagen = imagenFirma;
        firma.Estado = EstadoSolicitudFirma.Firmado;
        firma.FechaFirmaUtc = ahora.ToUniversalTime();
        firma.IpFirma = ip;
        firma.UserAgentFirma = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
        await context.SaveChangesAsync();

        return new(true, "Documento firmado correctamente.");
    }

    public static string NombreArchivo(string tipo, bool firmado)
    {
        var baseNombre = tipo switch
        {
            OptimeFirma.TipoContrato => "contrato-colaboracion-optime",
            OptimeFirma.TipoAdenda => "adenda-adhesion-pdv",
            _ => "codigo-buenas-practicas"
        };
        return firmado ? $"{baseNombre}-firmado.pdf" : $"{baseNombre}.pdf";
    }

    private string ObtenerBaseUrl(string? alternativa)
    {
        var baseUrl = _configuration["PublicSigning:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : alternativa;
        }
        return (baseUrl ?? string.Empty).TrimEnd('/');
    }

    private string ObtenerIp()
    {
        var context = _httpContextAccessor.HttpContext;
        var forwarded = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        var address = forwarded ?? context?.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(address)) return string.Empty;
        if (IPAddress.TryParse(address, out var parsed))
            return IPAddress.IsLoopback(parsed) ? "127.0.0.1" : parsed.MapToIPv4().ToString();
        return address;
    }

    // ---------- Generación de PDF sobre las plantillas de Resources ----------

    private static byte[] GenerarDocumento(string tipo, OptimeSolicitud s, byte[]? firma, string? ip, DateTime fecha)
    {
        var plantilla = tipo switch
        {
            OptimeFirma.TipoContrato => "optime_contrato_colaboracion.pdf",
            OptimeFirma.TipoAdenda => "optime_adenda_mayorista.pdf",
            _ => "optime_codigo_buenas_practicas.pdf"
        };

        using var document = PdfReader.Open(Path.Combine(AppContext.BaseDirectory, "Resources", plantilla), PdfDocumentOpenMode.Modify);
        EliminarCamposFormulario(document);

        switch (tipo)
        {
            case OptimeFirma.TipoContrato: RellenarContrato(document, s, firma, fecha); break;
            case OptimeFirma.TipoAdenda: RellenarAdenda(document, s, firma, fecha); break;
            default: RellenarCodigoBuenasPracticas(document, s, firma, fecha); break;
        }

        if (firma is { Length: > 0 })
            EstamparPieFirma(document, s, firma, ip, OptimeFirma.Titulo(tipo));

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static void RellenarContrato(PdfDocument document, OptimeSolicitud s, byte[]? firma, DateTime fecha)
    {
        var domicilio = $"{s.DireccionFiscal}, {s.CodigoPostal} {s.Poblacion} ({s.Provincia})";
        using (var g = XGraphics.FromPdfPage(document.Pages[0], XGraphicsPdfPageOptions.Append))
        {
            // Coordenadas de los campos originales del PDF (origen arriba-izquierda)
            Escribir(g, s.Poblacion, 441, 121, 58);
            Escribir(g, fecha.ToString("dd/MM/yyyy"), 514, 121, 70);
            Escribir(g, s.PersonaContacto, 204, 302, 306);
            Escribir(g, s.NifPersonaContacto, 143, 322, 108);
            Escribir(g, s.RazonSocial, 93, 346, 414);
            Escribir(g, s.CifNif, 339, 362, 154);
            Escribir(g, domicilio, 204, 383, 302);
            Escribir(g, s.RazonSocial, 170, 644, 320);
        }

        if (firma is { Length: > 0 })
        {
            DibujarFirma(document.Pages[8], firma, 298, 115, 150, 65);
            DibujarFirma(document.Pages[18], firma, 321, 509, 150, 65);
        }
    }

    private static void RellenarAdenda(PdfDocument document, OptimeSolicitud s, byte[]? firma, DateTime fecha)
    {
        using (var g = XGraphics.FromPdfPage(document.Pages[0], XGraphicsPdfPageOptions.Append))
        {
            Escribir(g, s.PersonaContacto, 128, 191, 122, negrita: true);
        }

        using (var g = XGraphics.FromPdfPage(document.Pages[2], XGraphicsPdfPageOptions.Append))
        {
            Escribir(g, s.RazonSocial, 168, 196, 370);
            Escribir(g, $"{s.DireccionFiscal}, {s.CodigoPostal} {s.Poblacion} ({s.Provincia})", 104, 211, 434);
            Escribir(g, s.Telefono, 142, 226, 396);
            g.DrawRectangle(XBrushes.White, 304, 339, 130, 16);
            Escribir(g, $"Sevilla {fecha.ToString("d 'de' MMMM 'del' yyyy", Es)}.", 306, 351, 230, negrita: true);
            Escribir(g, s.PersonaContacto, 222, 527, 316);
            Escribir(g, s.NifPersonaContacto, 80, 542, 200);
        }

        if (firma is { Length: > 0 })
            DibujarFirma(document.Pages[2], firma, 120, 458, 160, 42);
    }

    private static void RellenarCodigoBuenasPracticas(PdfDocument document, OptimeSolicitud s, byte[]? firma, DateTime fecha)
    {
        using (var g = XGraphics.FromPdfPage(document.Pages[0], XGraphicsPdfPageOptions.Append))
        {
            g.DrawRectangle(XBrushes.White, 118, 347, 180, 16);
            Escribir(g, $"Sevilla a {fecha.ToString("d 'de' MMMM 'de' yyyy", Es)}", 121, 359, 250, tamano: 11);
            Escribir(g, $"{s.NifPersonaContacto} - {s.PersonaContacto}", 118, 508, 400, tamano: 11);
        }

        if (firma is { Length: > 0 })
            DibujarFirma(document.Pages[0], firma, 135, 405, 160, 55);
    }

    private static void Escribir(XGraphics g, string? texto, double x, double yBase, double anchoMax, double tamano = 10, bool negrita = false)
    {
        if (string.IsNullOrWhiteSpace(texto)) return;
        texto = texto.Replace('\r', ' ').Replace('\n', ' ').Trim();
        var estilo = negrita ? XFontStyle.Bold : XFontStyle.Regular;
        var fuente = new XFont("Arial", tamano, estilo);
        while (g.MeasureString(texto, fuente).Width > anchoMax && tamano > 6)
        {
            tamano -= 0.5;
            fuente = new XFont("Arial", tamano, estilo);
        }
        g.DrawString(texto, fuente, XBrushes.Black, new XPoint(x, yBase));
    }

    private static void DibujarFirma(PdfPage pagina, byte[] firma, double x, double y, double ancho, double alto)
    {
        using var g = XGraphics.FromPdfPage(pagina, XGraphicsPdfPageOptions.Append);
        using var imagen = XImage.FromStream(() => new MemoryStream(firma));
        var escala = Math.Min(ancho / imagen.PointWidth, alto / imagen.PointHeight);
        var w = imagen.PointWidth * escala;
        var h = imagen.PointHeight * escala;
        g.DrawImage(imagen, x + (ancho - w) / 2, y + (alto - h) / 2, w, h);
    }

    private static void EstamparPieFirma(PdfDocument document, OptimeSolicitud s, byte[] firma, string? ip, string titulo)
    {
        var fuente = new XFont("Arial", 7, XFontStyle.Bold);
        var fondo = new XSolidBrush(XColor.FromArgb(240, 230, 255));
        foreach (var pagina in document.Pages)
        {
            using var g = XGraphics.FromPdfPage(pagina, XGraphicsPdfPageOptions.Append);
            var y = pagina.Height.Point - 32;
            g.DrawRectangle(fondo, 42, y, pagina.Width.Point - 84, 28);
            g.DrawString($"Firmado por: {s.PersonaContacto} ({s.NifPersonaContacto}) - {s.RazonSocial}", fuente, XBrushes.Black, new XPoint(50, y + 11));
            g.DrawString($"IP de firma: {ip ?? "-"} | {titulo}", fuente, XBrushes.Black, new XPoint(50, y + 22));
            using var imagen = XImage.FromStream(() => new MemoryStream(firma));
            var escala = Math.Min(80 / imagen.PointWidth, 24 / imagen.PointHeight);
            g.DrawImage(imagen, pagina.Width.Point - 46 - imagen.PointWidth * escala, y + 2, imagen.PointWidth * escala, imagen.PointHeight * escala);
        }
    }

    private static void EliminarCamposFormulario(PdfDocument document)
    {
        document.Internals.Catalog.Elements.Remove("/AcroForm");
        foreach (var pagina in document.Pages)
        {
            var anotaciones = pagina.Annotations;
            for (var i = anotaciones.Count - 1; i >= 0; i--)
            {
                if (anotaciones[i].Elements.GetName("/Subtype") == "/Widget")
                    anotaciones.Remove(anotaciones[i]);
            }
        }
    }
}
