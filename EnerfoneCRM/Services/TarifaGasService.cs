using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EnerfoneCRM.Services
{
    public class TarifaGasService
    {
        private readonly DbContextProvider _dbContextProvider;

        public TarifaGasService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<TarifaGas>> ObtenerTodasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasGas
                .OrderBy(t => t.Empresa)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<List<TarifaGas>> ObtenerActivasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasGas
                .Where(t => t.Activa)
                .OrderBy(t => t.Empresa)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<TarifaGas?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasGas.FindAsync(id);
        }

        public async Task<List<TarifaGas>> ObtenerPorEmpresaAsync(string empresa)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasGas
                .Where(t => t.Empresa == empresa)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerEmpresasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasGas
                .Select(t => t.Empresa)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }

        public async Task<(bool exito, string mensaje)> CrearAsync(TarifaGas tarifa)
        {
            try
            {
                NormalizarTarifa(tarifa);

                await using var context = _dbContextProvider.CreateDbContext();
                context.TarifasGas.Add(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa creada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la tarifa: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> ActualizarAsync(TarifaGas tarifa)
        {
            try
            {
                NormalizarTarifa(tarifa);

                await using var context = _dbContextProvider.CreateDbContext();
                context.TarifasGas.Update(tarifa);
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
                var tarifa = await context.TarifasGas.FindAsync(id);
                if (tarifa == null)
                {
                    return (false, "Tarifa no encontrada");
                }

                context.TarifasGas.Remove(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa eliminada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar la tarifa: {ex.Message}");
            }
        }

        private void NormalizarTarifa(TarifaGas tarifa)
        {
            tarifa.TerminoFijoGas = FormatearDecimal(tarifa.TerminoFijoGas);
            tarifa.TerminoVariableGas = FormatearDecimal(tarifa.TerminoVariableGas);
            tarifa.PvdSva = FormatearDecimal(tarifa.PvdSva);
        }

        private string? FormatearDecimal(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return valor;

            if (decimal.TryParse(valor.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var numero))
            {
                return numero.ToString("0.000000", new CultureInfo("es-ES"));
            }

            return valor;
        }
    }
}
