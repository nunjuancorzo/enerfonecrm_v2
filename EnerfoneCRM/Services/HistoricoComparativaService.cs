using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EnerfoneCRM.Services;

/// <summary>
/// Servicio para gestionar el histórico de comparativas
/// </summary>
public class HistoricoComparativaService
{
    private readonly DbContextProvider _dbContextProvider;

    public HistoricoComparativaService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<HistoricoComparativa>> ObtenerTodosAsync(int? usuarioId = null, int limite = 100)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        var query = context.HistoricoComparativas
            .Include(h => h.Usuario)
            .OrderByDescending(h => h.FechaComparativa)
            .AsQueryable();

        if (usuarioId.HasValue)
        {
            query = query.Where(h => h.UsuarioId == usuarioId.Value);
        }

        return await query.Take(limite).ToListAsync();
    }

    public async Task<HistoricoComparativa?> ObtenerPorIdAsync(int id)
    {
        using var context = _dbContextProvider.CreateDbContext();
        return await context.HistoricoComparativas
            .Include(h => h.Usuario)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<List<HistoricoComparativa>> BuscarAsync(
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? origen = null,
        string? tipoEnergia = null,
        int? usuarioId = null,
        int limite = 100)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        var query = context.HistoricoComparativas
            .Include(h => h.Usuario)
            .AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(h => h.FechaComparativa >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(h => h.FechaComparativa <= fechaHasta.Value);

        if (!string.IsNullOrEmpty(origen))
            query = query.Where(h => h.Origen == origen);

        if (!string.IsNullOrEmpty(tipoEnergia))
            query = query.Where(h => h.TipoEnergia == tipoEnergia);

        if (usuarioId.HasValue)
            query = query.Where(h => h.UsuarioId == usuarioId.Value);

        return await query
            .OrderByDescending(h => h.FechaComparativa)
            .Take(limite)
            .ToListAsync();
    }

    public async Task<(bool exito, string mensaje, int id)> GuardarComparativaAsync(
        string origen,
        string tipoEnergia,
        object datosUtilizados,
        object resultadoRanking,
        string? cups = null,
        string? emailCliente = null,
        string? comercializadoraActual = null,
        string? tarifaActual = null,
        decimal? totalFacturaActual = null,
        int? mejorTarifaId = null,
        string? mejorTarifaNombre = null,
        string? mejorTarifaEmpresa = null,
        decimal? ahorroMensual = null,
        decimal? ahorroAnual = null,
        decimal? porcentajeAhorro = null,
        string? proveedorOcr = null,
        string? advertencias = null,
        int? usuarioId = null,
        string? nombreArchivoFactura = null)
    {
        try
        {
            using var context = _dbContextProvider.CreateDbContext();

            var historico = new HistoricoComparativa
            {
                FechaComparativa = DateTime.Now,
                Origen = origen,
                EmailCliente = emailCliente,
                TipoEnergia = tipoEnergia,
                Cups = cups,
                ComercializadoraActual = comercializadoraActual,
                TarifaActual = tarifaActual,
                TotalFacturaActual = totalFacturaActual,
                MejorTarifaId = mejorTarifaId,
                MejorTarifaNombre = mejorTarifaNombre,
                MejorTarifaEmpresa = mejorTarifaEmpresa,
                AhorroMensual = ahorroMensual,
                AhorroAnual = ahorroAnual,
                PorcentajeAhorro = porcentajeAhorro,
                DatosUtilizados = JsonSerializer.Serialize(datosUtilizados),
                ResultadoRanking = JsonSerializer.Serialize(resultadoRanking),
                ProveedorOcr = proveedorOcr,
                EmailEnviado = false,
                Advertencias = advertencias,
                UsuarioId = usuarioId,
                NombreArchivoFactura = nombreArchivoFactura
            };

            context.HistoricoComparativas.Add(historico);
            await context.SaveChangesAsync();

            return (true, "Comparativa guardada en el histórico", historico.Id);
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar en histórico: {ex.Message}", 0);
        }
    }

    public async Task<(bool exito, string mensaje)> MarcarEmailEnviadoAsync(int id)
    {
        try
        {
            using var context = _dbContextProvider.CreateDbContext();
            
            var historico = await context.HistoricoComparativas.FindAsync(id);
            if (historico == null)
            {
                return (false, "Registro no encontrado");
            }

            historico.EmailEnviado = true;
            historico.FechaEnvioEmail = DateTime.Now;

            await context.SaveChangesAsync();
            return (true, "Estado de email actualizado");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar estado: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        try
        {
            using var context = _dbContextProvider.CreateDbContext();
            
            var historico = await context.HistoricoComparativas.FindAsync(id);
            if (historico == null)
            {
                return (false, "Registro no encontrado");
            }

            context.HistoricoComparativas.Remove(historico);
            await context.SaveChangesAsync();

            return (true, "Registro eliminado correctamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar: {ex.Message}");
        }
    }

    public async Task<Dictionary<string, int>> ObtenerEstadisticasAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        var query = context.HistoricoComparativas.AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(h => h.FechaComparativa >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(h => h.FechaComparativa <= fechaHasta.Value);

        var stats = new Dictionary<string, int>
        {
            ["total"] = await query.CountAsync(),
            ["frontend"] = await query.CountAsync(h => h.Origen == "frontend"),
            ["backend"] = await query.CountAsync(h => h.Origen == "backend"),
            ["con_email"] = await query.CountAsync(h => h.EmailEnviado),
            ["con_ocr"] = await query.CountAsync(h => !string.IsNullOrEmpty(h.ProveedorOcr)),
            ["luz"] = await query.CountAsync(h => h.TipoEnergia == "LUZ"),
            ["gas"] = await query.CountAsync(h => h.TipoEnergia == "GAS"),
            ["luz_gas"] = await query.CountAsync(h => h.TipoEnergia == "LUZ+GAS")
        };

        return stats;
    }
}
