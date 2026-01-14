using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class TarifaAlarmaService
{
    private readonly DbContextProvider _dbContextProvider;

    public TarifaAlarmaService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<TarifaAlarma>> ObtenerTodasAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.TarifasAlarmas
            .OrderBy(t => t.TipoInmueble)
            .ThenBy(t => t.Tipo)
            .ThenBy(t => t.NombreTarifa)
            .ToListAsync();
    }

    public async Task<List<TarifaAlarma>> ObtenerPorTipoAsync(string tipo)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.TarifasAlarmas
            .Where(t => t.Tipo == tipo && t.Activa)
            .OrderBy(t => t.NombreTarifa)
            .ToListAsync();
    }

    public async Task<List<TarifaAlarma>> ObtenerPorTipoInmuebleAsync(string tipoInmueble)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.TarifasAlarmas
            .Where(t => t.TipoInmueble == tipoInmueble && t.Activa)
            .OrderBy(t => t.Tipo)
            .ThenBy(t => t.NombreTarifa)
            .ToListAsync();
    }

    public async Task<TarifaAlarma?> ObtenerPorIdAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.TarifasAlarmas.FindAsync(id);
    }

    public async Task<(bool exito, string mensaje)> CrearAsync(TarifaAlarma tarifa)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            // Verificar si ya existe una tarifa con el mismo nombre
            var existe = await context.TarifasAlarmas
                .AnyAsync(t => t.NombreTarifa == tarifa.NombreTarifa && 
                              t.Tipo == tarifa.Tipo && 
                              t.TipoInmueble == tarifa.TipoInmueble);
            
            if (existe)
            {
                return (false, "Ya existe una tarifa con ese nombre, tipo e inmueble");
            }

            context.TarifasAlarmas.Add(tarifa);
            await context.SaveChangesAsync();
            return (true, "Tarifa creada exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al crear tarifa: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(TarifaAlarma tarifa)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var tarifaExistente = await context.TarifasAlarmas.FindAsync(tarifa.Id);
            if (tarifaExistente == null)
            {
                return (false, "Tarifa no encontrada");
            }

            // Verificar si el nuevo nombre ya existe en otra tarifa
            var nombreDuplicado = await context.TarifasAlarmas
                .AnyAsync(t => t.Id != tarifa.Id && 
                              t.NombreTarifa == tarifa.NombreTarifa && 
                              t.Tipo == tarifa.Tipo && 
                              t.TipoInmueble == tarifa.TipoInmueble);
            
            if (nombreDuplicado)
            {
                return (false, "Ya existe otra tarifa con ese nombre, tipo e inmueble");
            }

            tarifaExistente.Tipo = tarifa.Tipo;
            tarifaExistente.TipoInmueble = tarifa.TipoInmueble;
            tarifaExistente.NombreTarifa = tarifa.NombreTarifa;
            tarifaExistente.CuotaMensual = tarifa.CuotaMensual;
            tarifaExistente.Permanencia = tarifa.Permanencia;
            tarifaExistente.Empresa = tarifa.Empresa;
            tarifaExistente.Comision = tarifa.Comision;
            tarifaExistente.Descripcion = tarifa.Descripcion;
            tarifaExistente.Activa = tarifa.Activa;

            context.TarifasAlarmas.Update(tarifaExistente);
            await context.SaveChangesAsync();
            return (true, "Tarifa actualizada exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar tarifa: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var tarifa = await context.TarifasAlarmas.FindAsync(id);
            if (tarifa == null)
            {
                return (false, "Tarifa no encontrada");
            }

            context.TarifasAlarmas.Remove(tarifa);
            await context.SaveChangesAsync();
            return (true, "Tarifa eliminada exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar tarifa: {ex.Message}");
        }
    }

    public async Task<List<string>> ObtenerEmpresasAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.TarifasAlarmas
            .Where(t => !string.IsNullOrEmpty(t.Empresa))
            .Select(t => t.Empresa!)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();
    }
}
