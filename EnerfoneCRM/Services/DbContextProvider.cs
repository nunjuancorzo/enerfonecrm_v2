using EnerfoneCRM.Data;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

/// <summary>
/// Proveedor de DbContext que crea instancias nuevas con la base de datos correcta según el hostname.
/// Esto evita problemas de concurrencia en Blazor Server.
/// </summary>
public class DbContextProvider
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbContextProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Crea una nueva instancia de ApplicationDbContext configurada para la base de datos correcta.
    /// Debe usarse con 'using' o 'await using' para liberar recursos.
    /// </summary>
    public ApplicationDbContext CreateDbContext()
    {
        // Obtener hostname desde diferentes fuentes (IIS puede usar X-Forwarded-Host)
        var host = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-Host"].FirstOrDefault() 
                   ?? _httpContextAccessor.HttpContext?.Request.Headers["Host"].FirstOrDefault()
                   ?? _httpContextAccessor.HttpContext?.Request.Host.Host 
                   ?? "";
        
        // Determinar qué base de datos usar según el hostname
        var databaseName = host.StartsWith("demo.", StringComparison.OrdinalIgnoreCase) 
            ? "demoenerfone" 
            : "enerfone_pre";
        
        var baseConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        var connectionString = baseConnectionString.Replace("enerfone_pre", databaseName);
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
