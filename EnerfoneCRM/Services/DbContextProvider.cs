using EnerfoneCRM.Data;
using Microsoft.EntityFrameworkCore;
using EnerfoneCRM.Models;

namespace EnerfoneCRM.Services;

/// <summary>
/// Proveedor de DbContext que crea instancias nuevas con la base de datos correcta según el hostname.
/// Esto evita problemas de concurrencia en Blazor Server.
/// </summary>
public class DbContextProvider
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EmpresaContext _empresaContext;
    internal event Action? ContratosActualizados;

    public DbContextProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, EmpresaContext? empresaContext = null)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _empresaContext = empresaContext ?? new EmpresaContext();
    }

    /// <summary>
    /// Crea una nueva instancia de ApplicationDbContext configurada para la base de datos correcta.
    /// Debe usarse con 'using' o 'await using' para liberar recursos.
    /// </summary>
    public ApplicationDbContext CreateDbContext()
    {
        var context = new ApplicationDbContext(CrearOpciones(), _empresaContext.EmpresaId, _empresaContext.EsSuperAdmin);
        RegistrarEventos(context);
        return context;
    }

    internal void RegistrarEventos(ApplicationDbContext context)
    {
        context.ContratosActualizados += () => ContratosActualizados?.Invoke();
    }

    internal ApplicationDbContext CreateAuthenticationDbContext()
    {
        return new ApplicationDbContext(CrearOpciones(), autenticacion: true);
    }

    internal ApplicationDbContext CreatePublicDbContext()
    {
        return new ApplicationDbContext(CrearOpciones(), autenticacion: true, publico: true);
    }

    internal async Task<bool> ResolverEmpresaFirmaAsync<TEntity>(string token) where TEntity : class
    {
        if (typeof(TEntity) != typeof(SolicitudFirma) && typeof(TEntity) != typeof(SolicitudFirmaColaborador) && typeof(TEntity) != typeof(OptimeFirma))
            throw new InvalidOperationException("Tipo de firma no permitido.");
        if (string.IsNullOrWhiteSpace(token) || token.Length > 256) return false;
        await using var context = CreateAuthenticationDbContext();
        var hash = new FirmaTokenService().Hash(token);
        var empresaId = await context.Set<TEntity>()
            .Where(firma => EF.Property<string>(firma, "TokenHash") == hash)
            .Select(firma => (int?)EF.Property<int>(firma, "EmpresaId")).SingleOrDefaultAsync();
        if (!empresaId.HasValue || !await context.Empresas.AnyAsync(empresa => empresa.Id == empresaId && empresa.Activa)) return false;
        if (_empresaContext.EmpresaId > 0 && _empresaContext.EmpresaId != empresaId) return false;
        _empresaContext.Establecer(empresaId.Value, _empresaContext.EsSuperAdmin);
        return true;
    }

    private DbContextOptions<ApplicationDbContext> CrearOpciones()
    {
        // Obtener hostname desde diferentes fuentes (IIS puede usar X-Forwarded-Host)
        var host = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-Host"].FirstOrDefault() 
                   ?? _httpContextAccessor.HttpContext?.Request.Headers["Host"].FirstOrDefault()
                   ?? _httpContextAccessor.HttpContext?.Request.Host.Host 
                   ?? "";
        
        // Usar base de datos de producción
        var databaseName = "enerfone_pre";
        
        var baseConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        var connectionString = baseConnectionString.Replace("enerfone_pre", databaseName);
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        
        return optionsBuilder.Options;
    }
}
