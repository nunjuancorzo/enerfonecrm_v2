using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class LogActivacionContratoService
{
    private readonly DbContextProvider _dbContextProvider;

    public LogActivacionContratoService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<(bool exito, string mensaje)> RegistrarActivacionAsync(int contratoId, DateTime fechaActivacion, string? usuario, string? observaciones = null)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            
            var logActivacion = new LogActivacionContrato
            {
                ContratoId = contratoId,
                FechaActivacion = fechaActivacion,
                Usuario = usuario,
                Observaciones = observaciones,
                FechaRegistro = DateTime.Now
            };

            context.LogActivacionesContratos.Add(logActivacion);
            await context.SaveChangesAsync();

            return (true, "Activación registrada correctamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar la activación: {ex.Message}");
        }
    }

    public async Task<List<LogActivacionContrato>> ObtenerActivacionesPorContratoAsync(int contratoId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var activaciones = await context.LogActivacionesContratos
            .Where(l => l.ContratoId == contratoId)
            .OrderByDescending(l => l.FechaRegistro)
            .Select(l => new LogActivacionContrato
            {
                Id = l.Id,
                ContratoId = l.ContratoId,
                FechaActivacion = l.FechaActivacion,
                FechaRegistro = l.FechaRegistro,
                Usuario = l.Usuario ?? string.Empty,
                Observaciones = l.Observaciones ?? string.Empty
            })
            .ToListAsync();
        
        return activaciones;
    }

    public async Task<LogActivacionContrato?> ObtenerUltimaActivacionAsync(int contratoId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var activacion = await context.LogActivacionesContratos
            .Where(l => l.ContratoId == contratoId)
            .OrderByDescending(l => l.FechaRegistro)
            .Select(l => new LogActivacionContrato
            {
                Id = l.Id,
                ContratoId = l.ContratoId,
                FechaActivacion = l.FechaActivacion,
                FechaRegistro = l.FechaRegistro,
                Usuario = l.Usuario ?? string.Empty,
                Observaciones = l.Observaciones ?? string.Empty
            })
            .FirstOrDefaultAsync();
        
        return activacion;
    }

    public async Task<List<LogActivacionContrato>> ObtenerTodasActivacionesAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var activaciones = await context.LogActivacionesContratos
            .OrderByDescending(l => l.FechaRegistro)
            .Select(l => new LogActivacionContrato
            {
                Id = l.Id,
                ContratoId = l.ContratoId,
                FechaActivacion = l.FechaActivacion,
                FechaRegistro = l.FechaRegistro,
                Usuario = l.Usuario ?? string.Empty,
                Observaciones = l.Observaciones ?? string.Empty
            })
            .ToListAsync();
        
        return activaciones;
    }
}
