using System.Net;
using System.ComponentModel.DataAnnotations;
using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public sealed record SolicitudFirmaPublicaDto(
    string ProcesoId,
    string NombreCliente,
    string TipoContrato,
    string ReferenciaContrato,
    string Estado,
    DateTime FechaCaducidadUtc,
    int DocumentoOriginalId,
    int? DocumentoFirmadoId);

public sealed record ResultadoFirma(bool Exito, string Mensaje, string? Url = null);

public class FirmaService
{
    private readonly DbContextProvider _dbContextProvider;
    private readonly EmailService _emailService;
    private readonly ContractSigningPdfService _pdfService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<FirmaService> _logger;
    private readonly FirmaTokenService _tokenService = new();

    public FirmaService(
        DbContextProvider dbContextProvider,
        EmailService emailService,
        ContractSigningPdfService pdfService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FirmaService> logger)
    {
        _dbContextProvider = dbContextProvider;
        _emailService = emailService;
        _pdfService = pdfService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ResultadoFirma> CrearSolicitudAsync(int contratoId, bool esEnvioAutomatico = false)
    {
        if (!_configuration.GetValue("PublicSigning:Enabled", true))
        {
            return new(false, "La firma publica esta desactivada.");
        }

        await using var context = _dbContextProvider.CreateDbContext();
        var configuracionEmpresa = await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();
        if (esEnvioAutomatico && configuracionEmpresa != null && !configuracionEmpresa.EnvioDocumentosFirmaAutomatico)
        {
            return new(false, "El envío automático de documentos a firma está desactivado en la configuración de la empresa.");
        }

        var expirationDays = _configuration.GetValue<int?>("PublicSigning:TokenExpirationDays") ?? 30;
        if (expirationDays < 1 || expirationDays > 365)
        {
            return new(false, "La caducidad configurada no es valida.");
        }

        var contrato = await context.Contratos.FirstOrDefaultAsync(c => c.Id == contratoId);
        if (contrato == null)
        {
            return new(false, "Contrato no encontrado.");
        }

        var cliente = contrato.IdCliente.HasValue
            ? await context.Clientes.FirstOrDefaultAsync(c => c.Id == contrato.IdCliente.Value)
            : null;
        var email = cliente?.Email;
        if (cliente == null && string.IsNullOrWhiteSpace(contrato.NombreCliente))
        {
            return new(false, "El contrato no tiene un cliente asociado.");
        }

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
            return new(false, "El cliente no tiene un email valido.");
        }

        var documentoOriginal = await _pdfService.GenerarDocumentoOriginalAsync(contrato, cliente);
        var ahora = DateTime.UtcNow;
        var token = _tokenService.Generar();
        var solicitudActiva = await context.SolicitudesFirma
            .Where(s => s.ContratoId == contratoId && (s.Estado == EstadoSolicitudFirma.Enviado || s.Estado == EstadoSolicitudFirma.Pendiente || s.Estado == EstadoSolicitudFirma.EnProceso))
            .ToListAsync();
        var esReenvio = solicitudActiva.Count > 0;
        foreach (var anterior in solicitudActiva)
        {
            anterior.Estado = EstadoSolicitudFirma.Cancelado;
        }

        var documento = new FicheroContrato
        {
            IdContrato = contratoId,
            TipoFichero = "DocumentoFirmaOriginal",
            NombreFichero = $"contrato-{contratoId}-original.pdf",
            Fichero = documentoOriginal
        };
        context.FicherosContratos.Add(documento);
        await context.SaveChangesAsync();

        var solicitud = new SolicitudFirma
        {
            ContratoId = contratoId,
            ProcesoId = Guid.NewGuid().ToString("D"),
            TokenHash = _tokenService.Hash(token),
            FechaCreacionUtc = ahora,
            FechaCaducidadUtc = ahora.AddDays(expirationDays),
            Estado = EstadoSolicitudFirma.Enviado,
            EmailDestinatario = email.Trim(),
            NombreDestinatario = cliente?.Nombre ?? contrato.NombreCliente,
            DocumentoOriginalId = documento.Id,
            HashDocumentoOriginal = ContractSigningPdfService.CalcularHash(documentoOriginal)
        };
        context.SolicitudesFirma.Add(solicitud);
        await context.SaveChangesAsync();

        await RegistrarEventoAsync(solicitud.Id, esReenvio ? "EmailReenviado" : "SolicitudCreada", null, "OK", null);
        var url = ConstruirUrl(token);
        var nombre = WebUtility.HtmlEncode(solicitud.NombreDestinatario ?? "cliente");
        var fecha = solicitud.FechaCaducidadUtc.ToLocalTime().ToString("dd/MM/yyyy");
        var cuerpo = $"<p>Hola {nombre},</p><p>Tiene disponible la documentacion correspondiente a su contratacion.</p><p><a href=\"{WebUtility.HtmlEncode(url)}\">Consultar y firmar documentacion</a></p><p>Este enlace es personal y valido hasta el {fecha}.</p>";
        var emailResult = await _emailService.EnviarEmailSimpleAsync(
            solicitud.EmailDestinatario,
            "Documentacion pendiente de firma",
            cuerpo);
        await RegistrarEventoAsync(solicitud.Id, "EmailEnviado", null, emailResult.exito ? "OK" : "ERROR", emailResult.mensaje);

        if (!emailResult.exito)
        {
            return new(false, $"Solicitud creada, pero no se pudo enviar el email: {emailResult.mensaje}");
        }

        return new(true, "Documentacion enviada al cliente.", url);
    }

    public async Task<SolicitudFirmaPublicaDto?> ObtenerSolicitudPublicaAsync(string token)
    {
        if (!_configuration.GetValue("PublicSigning:Enabled", true))
        {
            return null;
        }

        var solicitud = await ObtenerSolicitudValidaAsync(token);
        if (solicitud == null)
        {
            return null;
        }

        if (solicitud.Estado is EstadoSolicitudFirma.Enviado or EstadoSolicitudFirma.Pendiente)
        {
            await CambiarEstadoAsync(solicitud.Id, EstadoSolicitudFirma.EnProceso, "EnlaceAbierto");
            solicitud.Estado = EstadoSolicitudFirma.EnProceso;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(solicitud.IpFirma))
            {
                await GuardarIpSolicitudAsync(solicitud.Id, ObtenerIp());
            }
            await RegistrarEventoAsync(solicitud.Id, "EnlaceAbierto", null, "OK", null);
        }
        await using var context = _dbContextProvider.CreateDbContext();
        var contrato = await context.Contratos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == solicitud.ContratoId);
        if (contrato == null)
        {
            return null;
        }

        return new SolicitudFirmaPublicaDto(
            solicitud.ProcesoId,
            solicitud.NombreDestinatario ?? contrato.NombreCliente ?? "cliente",
            contrato.Tipo ?? string.Empty,
            contrato.IdContratoExterno ?? contrato.Id.ToString(),
            solicitud.Estado,
            solicitud.FechaCaducidadUtc,
            solicitud.DocumentoOriginalId,
            solicitud.DocumentoFirmadoId);
    }

    public async Task<(byte[]? Contenido, string? Nombre, string Mensaje)> ObtenerDocumentoPublicoAsync(string token, int documentoId)
    {
        var solicitud = await ObtenerSolicitudValidaAsync(token, permitirEnProceso: true);
        if (solicitud == null)
        {
            return (null, null, "El enlace no es valido o ha caducado.");
        }

        var documentoPermitido = documentoId == solicitud.DocumentoOriginalId || documentoId == solicitud.DocumentoFirmadoId;
        if (!documentoPermitido)
        {
            return (null, null, "Documento no disponible.");
        }

        await using var context = _dbContextProvider.CreateDbContext();
        var documento = await context.FicherosContratos.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == documentoId && f.IdContrato == solicitud.ContratoId);
        if (documento == null)
        {
            return (null, null, "Documento no encontrado.");
        }

        await RegistrarEventoAsync(solicitud.Id, "DocumentoConsultado", documentoId, "OK", null);
        return (documento.Fichero, documento.NombreFichero, "OK");
    }

    public async Task<ResultadoFirma> CompletarFirmaAsync(string token, bool aceptaDocumentacion, byte[] firma)
    {
        if (!aceptaDocumentacion)
        {
            return new(false, "Debe aceptar la documentacion antes de firmar.");
        }

        if (firma.Length == 0 || firma.Length > 2 * 1024 * 1024)
        {
            return new(false, "La firma no es valida.");
        }

        var solicitud = await ObtenerSolicitudValidaAsync(token, permitirEnProceso: true);
        if (solicitud == null)
        {
            return new(false, "El enlace no es valido o ha caducado.");
        }

        await using var context = _dbContextProvider.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var actual = await context.SolicitudesFirma.FirstOrDefaultAsync(s => s.Id == solicitud.Id);
        if (actual == null || actual.Estado != EstadoSolicitudFirma.EnProceso || actual.FechaCaducidadUtc <= DateTime.UtcNow)
        {
            return new(false, "La solicitud ya no esta disponible.");
        }

        var contrato = await context.Contratos.FirstOrDefaultAsync(c => c.Id == actual.ContratoId);
        var cliente = contrato?.IdCliente == null ? null : await context.Clientes.FirstOrDefaultAsync(c => c.Id == contrato.IdCliente.Value);
        var original = await context.FicherosContratos.FirstOrDefaultAsync(f => f.Id == actual.DocumentoOriginalId && f.IdContrato == actual.ContratoId);
        if (contrato == null || original == null)
        {
            return new(false, "No se encontro la documentacion de la solicitud.");
        }

        var ipFirma = string.IsNullOrWhiteSpace(actual.IpFirma) ? ObtenerIp() : actual.IpFirma;
        var firmado = await _pdfService.GenerarDocumentoFirmadoAsync(original.Fichero, contrato, cliente, firma, ipFirma);
        var documentoFirmado = new FicheroContrato
        {
            IdContrato = actual.ContratoId,
            TipoFichero = "DocumentoFirmaFirmado",
            NombreFichero = $"contrato-{actual.ContratoId}-firmado.pdf",
            Fichero = firmado
        };
        context.FicherosContratos.Add(documentoFirmado);
        await context.SaveChangesAsync();
        actual.Estado = EstadoSolicitudFirma.Firmado;
        actual.FechaAceptacionUtc = DateTime.UtcNow;
        actual.FechaFirmaUtc = DateTime.UtcNow;
        actual.FirmaImagen = firma;
        actual.DocumentoFirmadoId = documentoFirmado.Id;
        actual.HashDocumentoFirmado = ContractSigningPdfService.CalcularHash(firmado);
        actual.IpFirma = ObtenerIp();
        actual.IpFirma = ipFirma;
        actual.UserAgentFirma = ObtenerUserAgent();
        if (contrato.Estado == "Pte Firma")
        {
            contrato.Estado = "Firmado";
            contrato.FechaModificacion = DateTime.Now;
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        await RegistrarEventoAsync(actual.Id, "FirmaCompletada", documentoFirmado.Id, "OK", null);
        return new(true, "La documentacion se ha firmado correctamente.");
    }

    public async Task<ResultadoFirma> CancelarSolicitudAsync(int contratoId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var solicitudes = await context.SolicitudesFirma
            .Where(s => s.ContratoId == contratoId && (s.Estado == EstadoSolicitudFirma.Enviado || s.Estado == EstadoSolicitudFirma.Pendiente || s.Estado == EstadoSolicitudFirma.EnProceso))
            .ToListAsync();
        if (solicitudes.Count == 0)
        {
            return new(false, "No hay una solicitud pendiente.");
        }

        foreach (var solicitud in solicitudes)
        {
            solicitud.Estado = EstadoSolicitudFirma.Cancelado;
        }

        await context.SaveChangesAsync();
        foreach (var solicitud in solicitudes)
        {
            await RegistrarEventoAsync(solicitud.Id, "SolicitudCancelada", null, "OK", null);
        }

        return new(true, "Solicitud cancelada.");
    }

    public async Task AbandonarSolicitudAsync(string token)
    {
        var solicitud = await ObtenerSolicitudValidaAsync(token, permitirEnProceso: true);
        if (solicitud == null || solicitud.Estado != EstadoSolicitudFirma.EnProceso)
        {
            return;
        }

        await CambiarEstadoAsync(solicitud.Id, EstadoSolicitudFirma.Caducado, "SolicitudCaducada");
    }

    public async Task<SolicitudFirma?> ObtenerUltimaSolicitudAsync(int contratoId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.SolicitudesFirma.AsNoTracking()
            .Where(s => s.ContratoId == contratoId)
            .OrderByDescending(s => s.FechaCreacionUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<HashSet<int>> ObtenerContratosFirmadosAsync(IEnumerable<int> contratoIds)
    {
        var ids = contratoIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new HashSet<int>();
        }

        await using var context = _dbContextProvider.CreateDbContext();
        var firmados = await context.SolicitudesFirma.AsNoTracking()
            .Where(s => ids.Contains(s.ContratoId) && s.Estado == EstadoSolicitudFirma.Firmado)
            .Select(s => s.ContratoId)
            .Distinct()
            .ToListAsync();
        return firmados.ToHashSet();
    }

    public async Task<ResultadoFirma> CrearSolicitudColaboradorAsync(int usuarioId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null) return new(false, "Colaborador no encontrado.");
        if (string.IsNullOrWhiteSpace(usuario.Email) || !new EmailAddressAttribute().IsValid(usuario.Email)) return new(false, "El colaborador no tiene un email valido.");
        var original = await _pdfService.GenerarDocumentoColaboradorOriginalAsync(usuario);
        var now = DateTime.UtcNow;
        var token = _tokenService.Generar();
        var anteriores = await context.SolicitudesFirmaColaboradores.Where(s => s.UsuarioId == usuarioId && s.Estado != EstadoSolicitudFirma.Firmado).ToListAsync();
        foreach (var anterior in anteriores) anterior.Estado = EstadoSolicitudFirma.Cancelado;
        var solicitud = new SolicitudFirmaColaborador
        {
            UsuarioId = usuarioId, ProcesoId = Guid.NewGuid().ToString("D"), TokenHash = _tokenService.Hash(token),
            FechaCreacionUtc = now, FechaCaducidadUtc = now.AddDays(_configuration.GetValue<int?>("PublicSigning:TokenExpirationDays") ?? 30),
            Estado = EstadoSolicitudFirma.Enviado, EmailDestinatario = usuario.Email,
            NombreDestinatario = string.Join(" ", new[] { usuario.Nombre, usuario.Apellidos }.Where(x => !string.IsNullOrWhiteSpace(x))),
            DocumentoOriginal = original, HashDocumentoOriginal = ContractSigningPdfService.CalcularHash(original)
        };
        context.SolicitudesFirmaColaboradores.Add(solicitud);
        await context.SaveChangesAsync();
        var baseUrl = _configuration["PublicSigning:BaseUrl"]?.TrimEnd('/') ?? "";
        var url = $"{baseUrl}/firma-colaborador/{Uri.EscapeDataString(token)}";
        var email = await _emailService.EnviarEmailSimpleAsync(usuario.Email, "Contrato de colaboracion pendiente de firma", $"<p>Hola {WebUtility.HtmlEncode(solicitud.NombreDestinatario)},</p><p>Puede consultar y firmar su contrato de colaboracion aqui:</p><p><a href=\"{WebUtility.HtmlEncode(url)}\">Consultar y firmar contrato</a></p>");
        return email.exito ? new(true, "Contrato enviado a firma.", url) : new(false, email.mensaje);
    }

    public async Task<SolicitudFirmaColaborador?> ObtenerSolicitudColaboradorAsync(string token)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var solicitud = await context.SolicitudesFirmaColaboradores.FirstOrDefaultAsync(s => s.TokenHash == _tokenService.Hash(token));
        if (solicitud == null || solicitud.Estado is EstadoSolicitudFirma.Cancelado or EstadoSolicitudFirma.Caducado) return null;
        if (solicitud.FechaCaducidadUtc <= DateTime.UtcNow) { solicitud.Estado = EstadoSolicitudFirma.Caducado; await context.SaveChangesAsync(); return null; }
        if (string.IsNullOrWhiteSpace(solicitud.IpFirma))
        {
            solicitud.IpFirma = ObtenerIp();
            await context.SaveChangesAsync();
        }
        return solicitud;
    }

    public async Task<SolicitudFirmaColaborador?> ObtenerUltimaSolicitudColaboradorAsync(int usuarioId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.SolicitudesFirmaColaboradores.AsNoTracking()
            .Where(s => s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.FechaCreacionUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<HashSet<int>> ObtenerUsuariosColaboradoresFirmadosAsync(IEnumerable<int> usuarioIds)
    {
        var ids = usuarioIds.Distinct().ToList();
        if (ids.Count == 0) return new HashSet<int>();
        await using var context = _dbContextProvider.CreateDbContext();
        var firmados = await context.SolicitudesFirmaColaboradores.AsNoTracking()
            .Where(s => ids.Contains(s.UsuarioId) && s.Estado == EstadoSolicitudFirma.Firmado)
            .Select(s => s.UsuarioId)
            .Distinct()
            .ToListAsync();
        return firmados.ToHashSet();
    }

    public async Task<(byte[]? Contenido, string? Nombre)> ObtenerDocumentoColaboradorPrivadoAsync(int usuarioId, bool firmado)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var solicitud = await context.SolicitudesFirmaColaboradores.AsNoTracking()
            .Where(s => s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.FechaCreacionUtc)
            .FirstOrDefaultAsync();
        var contenido = firmado ? solicitud?.DocumentoFirmado : solicitud?.DocumentoOriginal;
        return contenido == null ? (null, null) : (contenido, firmado ? "contrato-colaborador-firmado.pdf" : "contrato-colaborador.pdf");
    }

    public async Task<ResultadoFirma> CompletarFirmaColaboradorAsync(string token, byte[] firma)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var solicitud = await context.SolicitudesFirmaColaboradores.FirstOrDefaultAsync(s => s.TokenHash == _tokenService.Hash(token));
        if (solicitud == null || solicitud.Estado != EstadoSolicitudFirma.Enviado || solicitud.FechaCaducidadUtc <= DateTime.UtcNow) return new(false, "El enlace no es valido o ha caducado.");
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == solicitud.UsuarioId);
        if (usuario == null) return new(false, "Colaborador no encontrado.");
        var ipFirma = string.IsNullOrWhiteSpace(solicitud.IpFirma) ? ObtenerIp() : solicitud.IpFirma;
        var firmado = await _pdfService.GenerarDocumentoColaboradorFirmadoAsync(usuario, firma, ipFirma);
        solicitud.DocumentoFirmado = firmado; solicitud.FirmaImagen = firma; solicitud.Estado = EstadoSolicitudFirma.Firmado; solicitud.FechaFirmaUtc = DateTime.UtcNow; solicitud.HashDocumentoFirmado = ContractSigningPdfService.CalcularHash(firmado); solicitud.IpFirma = ipFirma; solicitud.UserAgentFirma = ObtenerUserAgent();
        await context.SaveChangesAsync();
        return new(true, "Contrato firmado correctamente.");
    }

    public async Task<(byte[]? Contenido, string? Nombre)> ObtenerDocumentoPrivadoAsync(int contratoId, int documentoId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var solicitud = await context.SolicitudesFirma.AsNoTracking()
            .Where(s => s.ContratoId == contratoId)
            .OrderByDescending(s => s.FechaCreacionUtc)
            .FirstOrDefaultAsync();
        if (solicitud == null || (documentoId != solicitud.DocumentoOriginalId && documentoId != solicitud.DocumentoFirmadoId))
        {
            return (null, null);
        }

        var documento = await context.FicherosContratos.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == documentoId && f.IdContrato == contratoId);
        return documento == null ? (null, null) : (documento.Fichero, documento.NombreFichero);
    }

    private async Task<SolicitudFirma?> ObtenerSolicitudValidaAsync(string token, bool permitirEnProceso = false)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 256)
        {
            return null;
        }

        await using var context = _dbContextProvider.CreateDbContext();
        var hash = _tokenService.Hash(token);
        var solicitud = await context.SolicitudesFirma.FirstOrDefaultAsync(s => s.TokenHash == hash);
        if (solicitud == null)
        {
            return null;
        }

        if (solicitud.FechaCaducidadUtc <= DateTime.UtcNow)
        {
            if (solicitud.Estado != EstadoSolicitudFirma.Firmado)
            {
                solicitud.Estado = EstadoSolicitudFirma.Caducado;
            }
            await context.SaveChangesAsync();
            await RegistrarEventoAsync(solicitud.Id, "SolicitudCaducada", null, "OK", null);
            return null;
        }

        if (solicitud.Estado == EstadoSolicitudFirma.EnProceso && !permitirEnProceso)
        {
            solicitud.Estado = EstadoSolicitudFirma.Caducado;
            await context.SaveChangesAsync();
            await RegistrarEventoAsync(solicitud.Id, "SolicitudCaducada", null, "OK", "Reapertura del enlace");
            return null;
        }

        return solicitud.Estado is EstadoSolicitudFirma.Enviado or EstadoSolicitudFirma.Pendiente or EstadoSolicitudFirma.EnProceso or EstadoSolicitudFirma.Firmado ? solicitud : null;
    }

    private async Task CambiarEstadoAsync(int solicitudId, string estado, string evento)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var solicitud = await context.SolicitudesFirma.FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud == null) return;
        solicitud.Estado = estado;
        await context.SaveChangesAsync();
        await RegistrarEventoAsync(solicitudId, evento, null, "OK", estado);
    }

    private async Task RegistrarEventoAsync(int solicitudId, string tipo, int? documentoId, string resultado, string? datos)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        context.SolicitudesFirmaEventos.Add(new SolicitudFirmaEvento
        {
            SolicitudFirmaId = solicitudId,
            TipoEvento = tipo,
            DocumentoId = documentoId,
            FechaHoraUtc = DateTime.UtcNow,
            Ip = ObtenerIp(),
            UserAgent = ObtenerUserAgent(),
            Resultado = resultado,
            Datos = datos
        });
        await context.SaveChangesAsync();
    }

    private string ConstruirUrl(string token)
    {
        var baseUrl = _configuration["PublicSigning:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            baseUrl = request == null ? string.Empty : $"{request.Scheme}://{request.Host}";
        }

        return $"{baseUrl}/firma/{Uri.EscapeDataString(token)}";
    }

    private string ObtenerIp()
    {
        var context = _httpContextAccessor.HttpContext;
        var forwarded = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        var address = forwarded ?? context?.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(address)) return string.Empty;
        if (System.Net.IPAddress.TryParse(address, out var parsed))
        {
            if (System.Net.IPAddress.IsLoopback(parsed)) return "127.0.0.1";
            return parsed.MapToIPv4().ToString();
        }
        return address;
    }

    private async Task GuardarIpSolicitudAsync(int solicitudId, string ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return;
        await using var context = _dbContextProvider.CreateDbContext();
        var solicitud = await context.SolicitudesFirma.FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud == null || !string.IsNullOrWhiteSpace(solicitud.IpFirma)) return;
        solicitud.IpFirma = ip;
        await context.SaveChangesAsync();
    }

    private string ObtenerUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty;
    }

}