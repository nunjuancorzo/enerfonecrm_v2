using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EnerfoneCRM.Services
{
    public class TarifaLuzService
    {
        private readonly DbContextProvider _dbContextProvider;

        public TarifaLuzService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<TarifaLuz>> ObtenerTodasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasLuz
                .OrderBy(t => t.Empresa)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<List<TarifaLuz>> ObtenerActivasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasLuz
                .Where(t => t.Activa)
                .OrderBy(t => t.Empresa)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<TarifaLuz?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasLuz.FindAsync(id);
        }

        public async Task<List<TarifaLuz>> ObtenerPorEmpresaAsync(string empresa)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasLuz
                .Where(t => t.Empresa == empresa)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerEmpresasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.TarifasLuz
                .Select(t => t.Empresa)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
        }

        public async Task<(bool exito, string mensaje)> CrearAsync(TarifaLuz tarifa)
        {
            try
            {
                NormalizarTarifa(tarifa);

                await using var context = _dbContextProvider.CreateDbContext();
                context.TarifasLuz.Add(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa creada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la tarifa: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> ActualizarAsync(TarifaLuz tarifa)
        {
            try
            {
                NormalizarTarifa(tarifa);

                await using var context = _dbContextProvider.CreateDbContext();
                context.TarifasLuz.Update(tarifa);
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
                var tarifa = await context.TarifasLuz.FindAsync(id);
                if (tarifa == null)
                {
                    return (false, "Tarifa no encontrada");
                }

                context.TarifasLuz.Remove(tarifa);
                await context.SaveChangesAsync();
                return (true, "Tarifa eliminada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar la tarifa: {ex.Message}");
            }
        }

        private void NormalizarTarifa(TarifaLuz tarifa)
        {
            tarifa.Potencia1 = FormatearDecimal(tarifa.Potencia1);
            tarifa.Potencia2 = FormatearDecimal(tarifa.Potencia2);
            tarifa.Potencia3 = FormatearDecimal(tarifa.Potencia3);
            tarifa.Potencia4 = FormatearDecimal(tarifa.Potencia4);
            tarifa.Potencia5 = FormatearDecimal(tarifa.Potencia5);
            tarifa.Potencia6 = FormatearDecimal(tarifa.Potencia6);

            tarifa.Energia1 = FormatearDecimal(tarifa.Energia1);
            tarifa.Energia2 = FormatearDecimal(tarifa.Energia2);
            tarifa.Energia3 = FormatearDecimal(tarifa.Energia3);
            tarifa.Energia4 = FormatearDecimal(tarifa.Energia4);
            tarifa.Energia5 = FormatearDecimal(tarifa.Energia5);
            tarifa.Energia6 = FormatearDecimal(tarifa.Energia6);
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
