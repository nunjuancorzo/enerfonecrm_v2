using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
                // Normalizar nombre de empresa (Title Case)
                NormalizarTarifa(tarifa);
                
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
                // Normalizar nombre de empresa (Title Case)
                NormalizarTarifa(tarifa);
                
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

        private void NormalizarTarifa(TarifaEnergia tarifa)
        {
            // Normalizar nombre de empresa a Title Case
            if (!string.IsNullOrWhiteSpace(tarifa.Empresa))
            {
                tarifa.Empresa = NormalizarNombreComercializadora(tarifa.Empresa);
            }

            // Formatear potencias con 6 decimales
            tarifa.Potencia1 = FormatearDecimal(tarifa.Potencia1);
            tarifa.Potencia2 = FormatearDecimal(tarifa.Potencia2);
            tarifa.Potencia3 = FormatearDecimal(tarifa.Potencia3);
            tarifa.Potencia4 = FormatearDecimal(tarifa.Potencia4);
            tarifa.Potencia5 = FormatearDecimal(tarifa.Potencia5);
            tarifa.Potencia6 = FormatearDecimal(tarifa.Potencia6);

            // Formatear energías con 6 decimales
            tarifa.Energia1 = FormatearDecimal(tarifa.Energia1);
            tarifa.Energia2 = FormatearDecimal(tarifa.Energia2);
            tarifa.Energia3 = FormatearDecimal(tarifa.Energia3);
            tarifa.Energia4 = FormatearDecimal(tarifa.Energia4);
            tarifa.Energia5 = FormatearDecimal(tarifa.Energia5);
            tarifa.Energia6 = FormatearDecimal(tarifa.Energia6);
        }

        private string NormalizarNombreComercializadora(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return nombre;

            // Convertir a Title Case (primera letra de cada palabra en mayúscula)
            var textInfo = new CultureInfo("es-ES", false).TextInfo;
            return textInfo.ToTitleCase(nombre.ToLower().Trim());
        }

        private string? FormatearDecimal(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return valor;

            // Intentar parsear el valor (reemplazando coma por punto para el parsing)
            if (decimal.TryParse(valor.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var numero))
            {
                // Formatear con 6 decimales y coma como separador
                return numero.ToString("0.000000", new CultureInfo("es-ES"));
            }

            return valor; // Si no se puede parsear, devolver el valor original
        }
    }
}
