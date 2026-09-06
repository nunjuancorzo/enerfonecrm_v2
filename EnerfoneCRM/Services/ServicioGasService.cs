using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class ServicioGasService
{
    private readonly DbContextProvider _dbContextProvider;

    public ServicioGasService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<ServicioGas>> ObtenerTodosAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.ServiciosGas
            .OrderBy(s => s.Tipo)
            .ThenBy(s => s.NombreServicio)
            .ToListAsync();
    }

    public async Task<List<ServicioGas>> ObtenerPorTipoAsync(string tipo)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.ServiciosGas
            .Where(s => s.Tipo == tipo)
            .OrderBy(s => s.NombreServicio)
            .ToListAsync();
    }

    public async Task<List<ServicioGas>> ObtenerPorTipoYComercializadoraAsync(string tipo, string nombreComercializadora)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.ServiciosGas
            .Where(s => s.Tipo == tipo && s.Empresa == nombreComercializadora)
            .OrderBy(s => s.NombreServicio)
            .ToListAsync();
    }

    public async Task<ServicioGas?> ObtenerPorIdAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.ServiciosGas.FindAsync(id);
    }

    public async Task<(bool exito, string mensaje)> CrearAsync(ServicioGas servicio)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        context.ServiciosGas.Add(servicio);
        await context.SaveChangesAsync();

        return (true, "Servicio creado exitosamente");
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(ServicioGas servicio)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        context.ServiciosGas.Update(servicio);
        await context.SaveChangesAsync();

        return (true, "Servicio actualizado exitosamente");
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var servicio = await context.ServiciosGas.FindAsync(id);
        if (servicio == null)
        {
            return (false, "Servicio no encontrado");
        }

        context.ServiciosGas.Remove(servicio);
        await context.SaveChangesAsync();

        return (true, "Servicio eliminado exitosamente");
    }
}
