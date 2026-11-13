using System.ComponentModel.DataAnnotations;
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

    public async Task<bool> ObtenerFormularioHabilitadoAsync(int empresaId)
    {
        await using var context = _dbContextProvider.CreateAuthenticationDbContext();
        var empresa = await context.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.Id == empresaId);
        return empresa?.FormularioOptimeHabilitado ?? false;
    }

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
        
        // Validar que la empresa exista
        if (solicitud.EmpresaId <= 0)
            throw new System.ComponentModel.DataAnnotations.ValidationException("La empresa no es válida");
        
        // Validar que el formulario esté habilitado para esta empresa
        await using var context = _dbContextProvider.CreatePublicDbContext();
        var empresa = await context.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.Id == solicitud.EmpresaId);
        if (empresa == null || !empresa.FormularioOptimeHabilitado)
            throw new System.ComponentModel.DataAnnotations.ValidationException("El formulario de Optime no está disponible para esta empresa");
        
        // Validar solo propiedades requeridas
        var requiredProps = new[] { nameof(OptimeSolicitud.RazonSocial), nameof(OptimeSolicitud.CifNif), 
            nameof(OptimeSolicitud.DireccionFiscal), nameof(OptimeSolicitud.Poblacion), 
            nameof(OptimeSolicitud.Provincia), nameof(OptimeSolicitud.CodigoPostal),
            nameof(OptimeSolicitud.PersonaContacto), nameof(OptimeSolicitud.NifPersonaContacto),
            nameof(OptimeSolicitud.Telefono), nameof(OptimeSolicitud.Email) };
        
        var tipoSolicitud = typeof(OptimeSolicitud);
        foreach (var propName in requiredProps)
        {
            var prop = tipoSolicitud.GetProperty(propName);
            var valor = prop?.GetValue(solicitud) as string;
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException(
                    $"El campo '{propName}' es requerido");
            }
        }
        
        // Validar formatos específicos
        if (!System.Text.RegularExpressions.Regex.IsMatch(solicitud.CodigoPostal, @"^\d{5}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El código postal debe tener 5 dígitos");
        if (!System.Text.RegularExpressions.Regex.IsMatch(solicitud.Telefono, @"^[\+]?[0-9\s\(\)\-]{8,25}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El teléfono no es válido");
        if (!System.Text.RegularExpressions.Regex.IsMatch(solicitud.Email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El email no es válido");
        
        // Validar campos opcionales si están presentes
        if (!string.IsNullOrWhiteSpace(solicitud.Cnae) && 
            !System.Text.RegularExpressions.Regex.IsMatch(solicitud.Cnae, @"^\d{1,10}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El CNAE debe ser numérico");
        if (!string.IsNullOrWhiteSpace(solicitud.Iban) && 
            !System.Text.RegularExpressions.Regex.IsMatch(solicitud.Iban, @"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El IBAN no es válido");
        if (!string.IsNullOrWhiteSpace(solicitud.Ccc) && 
            !System.Text.RegularExpressions.Regex.IsMatch(solicitud.Ccc, @"^\d{20}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El Nº CCC debe tener 20 dígitos");
        if (!string.IsNullOrWhiteSpace(solicitud.Swift) && 
            !System.Text.RegularExpressions.Regex.IsMatch(solicitud.Swift, @"^[A-Z0-9]{8,11}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El SWIFT debe tener 8 u 11 caracteres");
        
        solicitud.Id = 0;
        solicitud.FechaCreacion = DateTime.Now;
        solicitud.FechaModificacion = null;

        // Usar el mismo context para guardar
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
        
        // Validar formatos
        if (!string.IsNullOrWhiteSpace(solicitud.Cnae) && 
            !System.Text.RegularExpressions.Regex.IsMatch(solicitud.Cnae, @"^\d{1,10}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El CNAE debe ser numérico");
        if (!string.IsNullOrWhiteSpace(solicitud.Iban) && 
            !System.Text.RegularExpressions.Regex.IsMatch(solicitud.Iban, @"^ES\d{2}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El IBAN debe tener el formato ES00");
        if (!string.IsNullOrWhiteSpace(solicitud.Ccc) && 
            !System.Text.RegularExpressions.Regex.IsMatch(solicitud.Ccc, @"^\d{20}$"))
            throw new System.ComponentModel.DataAnnotations.ValidationException("El Nº CCC debe tener 20 dígitos");
        
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
