using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class OptimeService
{
    public const long MaxTamanoArchivo = 10 * 1024 * 1024;
    public const string ExtensionesPermitidas = ".pdf,.jpg,.jpeg,.png";

    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png"
    };

    private readonly DbContextProvider _dbContextProvider;

    public OptimeService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public static string? ObtenerContentType(string nombreArchivo) =>
        ContentTypes.TryGetValue(Path.GetExtension(nombreArchivo), out var ct) ? ct : null;

    public async Task<List<OptimeSolicitud>> ObtenerTodasAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.OptimeSolicitudes
            .AsNoTracking()
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<OptimeDocumento>> ObtenerDocumentosSinContenidoAsync(int solicitudId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.OptimeDocumentos
            .AsNoTracking()
            .Where(d => d.SolicitudId == solicitudId)
            .Select(d => new OptimeDocumento
            {
                Id = d.Id,
                SolicitudId = d.SolicitudId,
                Tipo = d.Tipo,
                NombreArchivo = d.NombreArchivo,
                ContentType = d.ContentType,
                FechaCreacion = d.FechaCreacion
            })
            .ToListAsync();
    }

    public async Task<OptimeDocumento?> ObtenerDocumentoAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.OptimeDocumentos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task CrearAsync(OptimeSolicitud solicitud, IEnumerable<OptimeDocumento> documentos)
    {
        Normalizar(solicitud);
        solicitud.Id = 0;
        solicitud.FechaCreacion = DateTime.Now;
        solicitud.FechaModificacion = null;

        await using var context = _dbContextProvider.CreateDbContext();
        await using var transaccion = await context.Database.BeginTransactionAsync();

        context.OptimeSolicitudes.Add(solicitud);
        await context.SaveChangesAsync();

        foreach (var doc in documentos)
        {
            doc.Id = 0;
            doc.SolicitudId = solicitud.Id;
            doc.FechaCreacion = DateTime.Now;
            context.OptimeDocumentos.Add(doc);
        }
        await context.SaveChangesAsync();
        await transaccion.CommitAsync();
    }

    public async Task<bool> ActualizarAsync(OptimeSolicitud solicitud)
    {
        Normalizar(solicitud);
        await using var context = _dbContextProvider.CreateDbContext();
        var existente = await context.OptimeSolicitudes.FindAsync(solicitud.Id);
        if (existente == null)
            return false;

        var fechaCreacion = existente.FechaCreacion;
        context.Entry(existente).CurrentValues.SetValues(solicitud);
        existente.FechaCreacion = fechaCreacion;
        existente.FechaModificacion = DateTime.Now;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task ReemplazarDocumentoAsync(int solicitudId, OptimeDocumento documento)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var anteriores = await context.OptimeDocumentos
            .Where(d => d.SolicitudId == solicitudId && d.Tipo == documento.Tipo)
            .ToListAsync();
        context.OptimeDocumentos.RemoveRange(anteriores);

        documento.Id = 0;
        documento.SolicitudId = solicitudId;
        documento.FechaCreacion = DateTime.Now;
        context.OptimeDocumentos.Add(documento);
        await context.SaveChangesAsync();
    }

    public async Task<bool> EliminarAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var existente = await context.OptimeSolicitudes.FindAsync(id);
        if (existente == null)
            return false;

        context.OptimeSolicitudes.Remove(existente);
        await context.SaveChangesAsync();
        return true;
    }

    private static void Normalizar(OptimeSolicitud s)
    {
        s.RazonSocial = s.RazonSocial.Trim();
        s.CifNif = s.CifNif.Trim().ToUpperInvariant();
        s.DireccionFiscal = s.DireccionFiscal.Trim();
        s.Poblacion = s.Poblacion.Trim();
        s.Provincia = s.Provincia.Trim();
        s.CodigoPostal = s.CodigoPostal.Trim();
        s.PersonaContacto = s.PersonaContacto.Trim();
        s.NifPersonaContacto = s.NifPersonaContacto.Trim().ToUpperInvariant();
        s.Telefono = s.Telefono.Trim();
        s.Email = s.Email.Trim();
        s.Cnae = Limpiar(s.Cnae);
        s.Iban = Limpiar(s.Iban)?.ToUpperInvariant();
        s.Ccc = Limpiar(s.Ccc);
        s.Swift = Limpiar(s.Swift)?.ToUpperInvariant();
        s.TitularCuenta = Limpiar(s.TitularCuenta);
    }

    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
