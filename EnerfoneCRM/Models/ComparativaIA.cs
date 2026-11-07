namespace EnerfoneCRM.Models;

public sealed record OfertaComparativaIA(string Compania, string Tarifa, decimal CosteMensual, decimal AhorroMensual,
    decimal AhorroAnual, decimal CostePotenciaPeriodo, decimal CosteEnergiaPeriodo);

public sealed record ComparativaIA(string Suministro, string Peaje, decimal DiasFacturacion,
    decimal TotalFacturaPeriodo, decimal CosteActualMensual, decimal IvaPorcentaje, IReadOnlyList<OfertaComparativaIA> Ofertas);