using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class ServicioService
{
    private readonly DbContextProvider _dbContextProvider;

    public ServicioService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<Servicio>> ObtenerTodosAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Servicios
            .OrderBy(s => s.Tipo)
            .ThenBy(s => s.NombreServicio)
            .ToListAsync();
    }

    public async Task<List<Servicio>> ObtenerPorTipoAsync(string tipo)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Servicios
            .Where(s => s.Tipo == tipo)
            .OrderBy(s => s.NombreServicio)
            .ToListAsync();
    }

    public async Task<List<Servicio>> ObtenerPorTipoYComercializadoraAsync(string tipo, string nombreComercializadora)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Servicios
            .Where(s => s.Tipo == tipo && s.Empresa == nombreComercializadora)
            .OrderBy(s => s.NombreServicio)
            .ToListAsync();
    }

    public async Task<Servicio?> ObtenerPorIdAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Servicios.FindAsync(id);
    }

    public async Task<(bool exito, string mensaje)> CrearAsync(Servicio servicio)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        context.Servicios.Add(servicio);
        await context.SaveChangesAsync();

        return (true, "Servicio creado exitosamente");
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(Servicio servicio)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        context.Servicios.Update(servicio);
        await context.SaveChangesAsync();

        return (true, "Servicio actualizado exitosamente");
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var servicio = await context.Servicios.FindAsync(id);
        if (servicio == null)
        {
            return (false, "Servicio no encontrado");
        }

        context.Servicios.Remove(servicio);
        await context.SaveChangesAsync();

        return (true, "Servicio eliminado exitosamente");
    }
}
