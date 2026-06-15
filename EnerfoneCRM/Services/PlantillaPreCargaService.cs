using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EnerfoneCRM.Services;

/// <summary>
/// Servicio para gestionar plantillas de PreCarga (mapeo visual de facturas)
/// </summary>
public class PlantillaPreCargaService
{
    private readonly DbContextProvider _dbContextProvider;

    public PlantillaPreCargaService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<PlantillaPreCarga>> ObtenerTodasAsync()
    {
        using var context = _dbContextProvider.CreateDbContext();
        return await context.PlantillasPreCarga
            .Include(p => p.UsuarioCreador)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<PlantillaPreCarga>> ObtenerActivasAsync()
    {
        using var context = _dbContextProvider.CreateDbContext();
        return await context.PlantillasPreCarga
            .Where(p => p.Activa)
            .OrderBy(p => p.Prioridad)
            .ThenBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<PlantillaPreCarga?> ObtenerPorIdAsync(int id)
    {
        using var context = _dbContextProvider.CreateDbContext();
        return await context.PlantillasPreCarga
            .Include(p => p.UsuarioCreador)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlantillaPreCarga?> BuscarPlantillaParaFacturaAsync(string comercializadora, string tipoEnergia)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        // Buscar plantillas activas para esa comercializadora y tipo de energía
        var plantillas = await context.PlantillasPreCarga
            .Where(p => p.Activa && p.TipoEnergia == tipoEnergia)
            .Where(p => p.Comercializadora.ToLower() == comercializadora.ToLower() ||
                        (p.AliasComercializadora != null && p.AliasComercializadora.ToLower().Contains(comercializadora.ToLower())))
            .OrderBy(p => p.Prioridad)
            .ToListAsync();

        return plantillas.FirstOrDefault();
    }

    public async Task<(bool exito, string mensaje, PlantillaPreCarga? plantilla)> CrearAsync(PlantillaPreCarga plantilla)
    {
        try
        {
            using var context = _dbContextProvider.CreateDbContext();
            
            // Verificar que no exista una plantilla duplicada
            var existente = await context.PlantillasPreCarga
                .AnyAsync(p => p.Comercializadora == plantilla.Comercializadora && 
                              p.TipoEnergia == plantilla.TipoEnergia &&
                              p.VarianteFactura == plantilla.VarianteFactura);

            if (existente)
            {
                return (false, "Ya existe una plantilla similar para esta comercializadora", null);
            }

            plantilla.FechaCreacion = DateTime.Now;
            context.PlantillasPreCarga.Add(plantilla);
            await context.SaveChangesAsync();

            return (true, "Plantilla creada correctamente", plantilla);
        }
        catch (Exception ex)
        {
            return (false, $"Error al crear plantilla: {ex.Message}", null);
        }
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(PlantillaPreCarga plantilla)
    {
        try
        {
            using var context = _dbContextProvider.CreateDbContext();
            
            var existente = await context.PlantillasPreCarga.FindAsync(plantilla.Id);
            if (existente == null)
            {
                return (false, "Plantilla no encontrada");
            }

            existente.Nombre = plantilla.Nombre;
            existente.Comercializadora = plantilla.Comercializadora;
            existente.AliasComercializadora = plantilla.AliasComercializadora;
            existente.TipoEnergia = plantilla.TipoEnergia;
            existente.VarianteFactura = plantilla.VarianteFactura;
            existente.Prioridad = plantilla.Prioridad;
            existente.Activa = plantilla.Activa;
            existente.CamposMapeados = plantilla.CamposMapeados;
            existente.NotasInternas = plantilla.NotasInternas;
            existente.ArchivoFacturaEjemplo = plantilla.ArchivoFacturaEjemplo;
            existente.FechaModificacion = DateTime.Now;

            await context.SaveChangesAsync();
            return (true, "Plantilla actualizada correctamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar plantilla: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        try
        {
            using var context = _dbContextProvider.CreateDbContext();
            
            var plantilla = await context.PlantillasPreCarga.FindAsync(id);
            if (plantilla == null)
            {
                return (false, "Plantilla no encontrada");
            }

            context.PlantillasPreCarga.Remove(plantilla);
            await context.SaveChangesAsync();

            return (true, "Plantilla eliminada correctamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar plantilla: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> GuardarCamposMapeadosAsync(int plantillaId, List<CampoMapeadoPreCarga> campos)
    {
        try
        {
            using var context = _dbContextProvider.CreateDbContext();
            
            var plantilla = await context.PlantillasPreCarga.FindAsync(plantillaId);
            if (plantilla == null)
            {
                return (false, "Plantilla no encontrada");
            }

            plantilla.CamposMapeados = JsonSerializer.Serialize(campos, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            plantilla.FechaModificacion = DateTime.Now;

            await context.SaveChangesAsync();
            return (true, "Campos mapeados guardados correctamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar campos: {ex.Message}");
        }
    }

    public List<CampoMapeadoPreCarga>? ObtenerCamposMapeados(PlantillaPreCarga plantilla)
    {
        if (string.IsNullOrEmpty(plantilla.CamposMapeados))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<CampoMapeadoPreCarga>>(plantilla.CamposMapeados);
        }
        catch
        {
            return null;
        }
    }
}
