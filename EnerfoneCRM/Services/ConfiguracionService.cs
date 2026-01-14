using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class ConfiguracionService
{
    private readonly DbContextProvider _dbContextProvider;

    public ConfiguracionService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<ConfiguracionEmpresa?> ObtenerConfiguracionAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();
    }

    public async Task<(bool exito, string mensaje)> GuardarConfiguracionAsync(ConfiguracionEmpresa configuracion)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var existente = await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();
            
            if (existente != null)
            {
                // Actualizar existente
                existente.NombreEmpresa = configuracion.NombreEmpresa;
                existente.Cif = configuracion.Cif;
                existente.Direccion = configuracion.Direccion;
                existente.CodigoPostal = configuracion.CodigoPostal;
                existente.Ciudad = configuracion.Ciudad;
                existente.Provincia = configuracion.Provincia;
                existente.Pais = configuracion.Pais;
                existente.Telefono = configuracion.Telefono;
                existente.Email = configuracion.Email;
                existente.Web = configuracion.Web;
                existente.LogoUrl = configuracion.LogoUrl;
                
                context.ConfiguracionesEmpresa.Update(existente);
            }
            else
            {
                // Crear nuevo
                context.ConfiguracionesEmpresa.Add(configuracion);
            }
            
            await context.SaveChangesAsync();
            return (true, "Configuración guardada exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar configuración: {ex.Message}");
        }
    }
}
