using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class LogAccesoService
{
    private readonly DbContextProvider _dbContextProvider;

    public LogAccesoService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task RegistrarAccesoAsync(int idUsuario, string nombreUsuario, string rol)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var logAcceso = new LogAcceso
            {
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                Rol = rol,
                FechaAcceso = DateTime.Now
            };

            context.LogAccesos.Add(logAcceso);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log del error pero no interrumpir el proceso de login
            Console.WriteLine($"Error al registrar acceso: {ex.Message}");
        }
    }

    public async Task<List<LogAcceso>> ObtenerTodosAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.LogAccesos
            .OrderByDescending(l => l.FechaAcceso)
            .ToListAsync();
    }

    public async Task<List<LogAcceso>> ObtenerPorFiltrosAsync(string? nombreUsuario = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var query = context.LogAccesos.AsQueryable();

        if (!string.IsNullOrEmpty(nombreUsuario))
        {
            query = query.Where(l => l.NombreUsuario.Contains(nombreUsuario));
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(l => l.FechaAcceso >= fechaInicio.Value);
        }

        if (fechaFin.HasValue)
        {
            // Incluir todo el día final
            var fechaFinConHora = fechaFin.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(l => l.FechaAcceso <= fechaFinConHora);
        }

        return await query
            .OrderByDescending(l => l.FechaAcceso)
            .ToListAsync();
    }

    public async Task<int> ObtenerTotalAccesosUsuarioAsync(int idUsuario)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.LogAccesos
            .Where(l => l.IdUsuario == idUsuario)
            .CountAsync();
    }

    public async Task<DateTime?> ObtenerUltimoAccesoUsuarioAsync(int idUsuario)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var ultimoLog = await context.LogAccesos
            .Where(l => l.IdUsuario == idUsuario)
            .OrderByDescending(l => l.FechaAcceso)
            .FirstOrDefaultAsync();

        return ultimoLog?.FechaAcceso;
    }
}
