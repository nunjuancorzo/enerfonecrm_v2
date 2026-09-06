using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;
using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class HistoricoCambioContratoService
{
    private readonly DbContextProvider _dbContextProvider;

    // Campos técnicos que no aportan información al histórico
    private static readonly HashSet<string> PropiedadesExcluidas = new()
    {
        nameof(Contrato.Id),
        nameof(Contrato.FechaCreacion),
        nameof(Contrato.FechaModificacion)
    };

    private static readonly Dictionary<string, string> EtiquetasCampos = new()
    {
        [nameof(Contrato.EnComercializadora)] = "Comercializadora",
        [nameof(Contrato.EnTarifa)] = "Tarifa Luz",
        [nameof(Contrato.EnTarifaId)] = "Tarifa Luz (Id)",
        [nameof(Contrato.EnTarifaGas)] = "Tarifa Gas",
        [nameof(Contrato.EnTarifaGasId)] = "Tarifa Gas (Id)",
        [nameof(Contrato.EnCups)] = "CUPS Luz",
        [nameof(Contrato.EnCupsGas)] = "CUPS Gas",
        [nameof(Contrato.ServicioId)] = "Servicio Luz",
        [nameof(Contrato.ServicioGasId)] = "Servicio Gas",
        [nameof(Contrato.EstadoServicio)] = "Estado Servicio Luz",
        [nameof(Contrato.EstadoServicioGas)] = "Estado Servicio Gas",
        [nameof(Contrato.ConsumoAnual)] = "Consumo Anual Luz",
        [nameof(Contrato.ConsumoAnualGas)] = "Consumo Anual Gas",
        [nameof(Contrato.EnIban)] = "IBAN Energía",
        [nameof(Contrato.Dni)] = "DNI"
    };

    public HistoricoCambioContratoService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<HistoricoCambioContrato>> ObtenerPorContratoAsync(int idContrato)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.HistoricoCambiosContratos
            .Where(h => h.IdContrato == idContrato)
            .OrderByDescending(h => h.FechaCambio)
            .ThenByDescending(h => h.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Compara dos versiones del contrato y guarda una entrada por cada campo modificado.
    /// </summary>
    public async Task<int> RegistrarCambiosAsync(Contrato original, Contrato modificado, string? usuario)
    {
        try
        {
            var cambios = ObtenerCambios(original, modificado, usuario);
            if (cambios.Count == 0) return 0;

            await using var context = _dbContextProvider.CreateDbContext();
            context.HistoricoCambiosContratos.AddRange(cambios);
            await context.SaveChangesAsync();
            return cambios.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HistoricoCambioContratoService] Error al registrar cambios: {ex.Message}");
            return 0;
        }
    }

    private List<HistoricoCambioContrato> ObtenerCambios(Contrato original, Contrato modificado, string? usuario)
    {
        var cambios = new List<HistoricoCambioContrato>();
        var ahora = DateTime.Now;

        var propiedades = typeof(Contrato)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !PropiedadesExcluidas.Contains(p.Name))
            .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null);

        foreach (var propiedad in propiedades)
        {
            var valorOriginal = propiedad.GetValue(original);
            var valorNuevo = propiedad.GetValue(modificado);

            if (SonEquivalentes(valorOriginal, valorNuevo)) continue;

            cambios.Add(new HistoricoCambioContrato
            {
                IdContrato = modificado.Id,
                Campo = ObtenerEtiqueta(propiedad.Name),
                Dato = FormatearValor(valorNuevo),
                Usuario = usuario ?? "Sistema",
                FechaCambio = ahora
            });
        }

        return cambios;
    }

    private static bool SonEquivalentes(object? a, object? b)
    {
        // Cadenas vacías y nulos se consideran el mismo valor
        var textoA = a as string;
        var textoB = b as string;
        if (a is string || b is string)
        {
            return string.Equals(textoA?.Trim() ?? string.Empty, textoB?.Trim() ?? string.Empty, StringComparison.Ordinal);
        }

        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        return a.Equals(b);
    }

    private static string? FormatearValor(object? valor)
    {
        var texto = valor switch
        {
            null => string.Empty,
            bool b => b ? "Sí" : "No",
            DateTime f => f.ToString("dd/MM/yyyy"),
            decimal d => d.ToString("N2"),
            _ => valor.ToString() ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(texto)) texto = "(vacío)";
        return texto.Length > 500 ? texto[..500] : texto;
    }

    private static string ObtenerEtiqueta(string nombrePropiedad)
    {
        if (EtiquetasCampos.TryGetValue(nombrePropiedad, out var etiqueta))
            return etiqueta;

        // "PotenciaContratadaP1" -> "Potencia Contratada P1"
        var separado = Regex.Replace(nombrePropiedad, "(?<=[a-z0-9])([A-Z])", " $1");
        return separado.Length > 100 ? separado[..100] : separado;
    }
}
