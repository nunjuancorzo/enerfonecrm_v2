using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class EstadoServicioContratoService
{
    private readonly DbContextProvider _dbContextProvider;

    public EstadoServicioContratoService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<Dictionary<string, EstadoServicioContrato>> ObtenerPorContratoAsync(int contratoId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.EstadosServicioContrato.AsNoTracking()
            .Where(estado => estado.ContratoId == contratoId)
            .ToDictionaryAsync(estado => estado.ClaveLinea);
    }

    public async Task GuardarAsync(int contratoId, IEnumerable<EstadoServicioContrato> estados)
    {
        var estadosPorClave = estados
            .Where(estado => !string.IsNullOrWhiteSpace(estado.ClaveLinea))
            .GroupBy(estado => estado.ClaveLinea, StringComparer.Ordinal)
            .Select(grupo => grupo.Last())
            .ToDictionary(estado => estado.ClaveLinea, StringComparer.Ordinal);

        await using var context = _dbContextProvider.CreateDbContext();
        var existentes = await context.EstadosServicioContrato
            .Where(estado => estado.ContratoId == contratoId)
            .ToListAsync();

        foreach (var existente in existentes)
        {
            if (!estadosPorClave.Remove(existente.ClaveLinea, out var actualizado))
            {
                context.EstadosServicioContrato.Remove(existente);
                continue;
            }

            existente.Estado = actualizado.Estado;
            existente.FechaInstalacion = actualizado.FechaInstalacion;
        }

        foreach (var nuevo in estadosPorClave.Values)
        {
            context.EstadosServicioContrato.Add(new EstadoServicioContrato
            {
                ContratoId = contratoId,
                ClaveLinea = nuevo.ClaveLinea,
                Estado = nuevo.Estado,
                FechaInstalacion = nuevo.FechaInstalacion
            });
        }

        await context.SaveChangesAsync();
    }
}