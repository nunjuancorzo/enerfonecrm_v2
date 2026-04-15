using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class TarifaTelefoniaService
    {
        private readonly DbContextProvider _dbContextProvider;

        public TarifaTelefoniaService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<TarifaTelefonia>> ObtenerTodasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasTelefonia
                .OrderBy(t => t.Compania)
                .ThenBy(t => t.Tipo)
                .ToListAsync();
        }

        public async Task<TarifaTelefonia?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasTelefonia.FindAsync(id);
        }

        public async Task<(bool exito, string mensaje)> CrearAsync(TarifaTelefonia tarifa)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                context.TarifasTelefonia.Add(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa creada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la tarifa: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> ActualizarAsync(TarifaTelefonia tarifa)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                context.TarifasTelefonia.Update(tarifa);
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
                var tarifa = await context.TarifasTelefonia.FindAsync(id);
                if (tarifa == null)
                {
                    return (false, "Tarifa no encontrada");
                }

                context.TarifasTelefonia.Remove(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa eliminada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar la tarifa: {ex.Message}");
            }
        }

        public async Task<List<string>> ObtenerCompaniasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasTelefonia
                .Select(t => t.Compania)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerTiposAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasTelefonia
                .Select(t => t.Tipo)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<List<TarifaTelefonia>> ObtenerPorCompaniaYTipoAsync(string compania, string tipo)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasTelefonia
                .Where(t => t.Compania == compania && t.Tipo == tipo)
                .OrderBy(t => t.PrecioNew)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerEmpresasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasTelefonia
                .Select(t => t.Compania)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<TarifaTelefonia>> ObtenerPorEmpresaAsync(string empresa)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasTelefonia
                .Where(t => t.Compania == empresa)
                .OrderBy(t => t.Tipo)
                .ToListAsync();
        }
    }
}
