using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class TarifaEnergiaService
    {
        private readonly DbContextProvider _dbContextProvider;

        public TarifaEnergiaService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<TarifaEnergia>> ObtenerTodasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasEnergia
                .OrderBy(t => t.Empresa)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<TarifaEnergia?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasEnergia.FindAsync(id);
        }

        public async Task<(bool exito, string mensaje)> CrearAsync(TarifaEnergia tarifa)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                context.TarifasEnergia.Add(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa creada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la tarifa: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> ActualizarAsync(TarifaEnergia tarifa)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                context.TarifasEnergia.Update(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa actualizada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar la tarifa: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                var tarifa = await context.TarifasEnergia.FindAsync(id);
                if (tarifa == null)
                {
                    return (false, "Tarifa no encontrada");
                }

                context.TarifasEnergia.Remove(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa eliminada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar la tarifa: {ex.Message}");
            }
        }

        public async Task<List<string>> ObtenerEmpresasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasEnergia
                .Select(t => t.Empresa)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerTiposAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasEnergia
                .Select(t => t.Tipo)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<List<TarifaEnergia>> ObtenerPorEmpresaAsync(string empresa)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasEnergia
                .Where(t => t.Empresa == empresa)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }
    }
}
