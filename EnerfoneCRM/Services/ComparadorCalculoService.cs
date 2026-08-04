using System;
using System.Collections.Generic;
using System.Linq;

namespace EnerfoneCRM.Services
{
    /// <summary>
    /// Servicio para cálculos de comparativas eléctricas siguiendo el algoritmo documentado
    /// en ALGORITMO_CALCULO_OFERTAS.md
    /// </summary>
    public class ComparadorCalculoService
    {
        private static readonly string[] PERIODS = { "P1", "P2", "P3", "P4", "P5", "P6" };

        /// <summary>
        /// Calcula una comparativa entre la factura actual del cliente y una tarifa Naturgy
        /// </summary>
        public ResultadoCalculoComparativa CalcularComparativa(
            DatosFacturaActual facturaActual,
            DatosTarifaNaturgy tarifaNaturgy,
            ReglasComparativa? reglas = null)
        {
            reglas ??= ReglasComparativa.Default();

            var resultado = new ResultadoCalculoComparativa();

            try
            {
                // PASO 1: Determinar periodos según peaje
                var (periodosPotencia, periodosEnergia) = ObtenerPeriodosPorPeaje(tarifaNaturgy.Peaje);
                resultado.PowerPeriodCount = periodosPotencia;
                resultado.EnergyPeriodCount = periodosEnergia;

                // PASO 2: Calcular días del periodo
                var dias = CalcularDias(facturaActual.FechaInicio, facturaActual.FechaFin);
                resultado.Days = dias;

                // PASO 3: Cálculo de costes de POTENCIA
                var (potenciaActual, potenciaNaturgy, lineasPotencia) = CalcularCostePotencia(
                    facturaActual, tarifaNaturgy, periodosPotencia, dias);
                resultado.PowerLines = lineasPotencia;

                // PASO 4: Cálculo de costes de ENERGÍA
                var (energiaActual, energiaNaturgy, lineasEnergia) = CalcularCosteEnergia(
                    facturaActual, tarifaNaturgy, periodosEnergia);
                resultado.EnergyLines = lineasEnergia;

                // PASO 5: Aplicar reglas a conceptos adicionales
                var conceptosActual = facturaActual.Conceptos ?? new ConceptosAdicionales();
                var conceptosNaturgy = AplicarReglasConceptos(conceptosActual, reglas);

                // PASO 6: Cálculo de excedentes
                decimal excedentesActual = 0, excedentesNaturgy = 0;
                if (facturaActual.ExcedentesKwh > 0)
                {
                    excedentesActual = -(facturaActual.ExcedentesKwh * conceptosActual.PrecioExcedente);
                    excedentesNaturgy = -(facturaActual.ExcedentesKwh * reglas.PrecioExcedentesNaturgy);
                }

                // PASO 7: Cálculo del impuesto eléctrico
                decimal baseImpuestoActual = potenciaActual + energiaActual;
                decimal baseImpuestoNaturgy = potenciaNaturgy + energiaNaturgy;
                
                decimal impuestoActual = baseImpuestoActual * (reglas.PorcentajeImpuestoElectrico / 100);
                decimal impuestoNaturgy = baseImpuestoNaturgy * (reglas.PorcentajeImpuestoElectrico / 100);

                // PASO 8: Cálculo de base imponible (antes de IVA)
                decimal baseActual = 
                    conceptosActual.ExcesoPotencia +
                    potenciaActual +
                    energiaActual +
                    conceptosActual.EnergiaReactiva +
                    conceptosActual.DerechosExtension +
                    conceptosActual.DerechosAcceso +
                    impuestoActual +
                    conceptosActual.BonoSocial +
                    conceptosActual.Descuentos +
                    conceptosActual.AlquilerEquipos +
                    excedentesActual;

                decimal baseNaturgy = 
                    conceptosNaturgy.ExcesoPotencia +
                    potenciaNaturgy +
                    energiaNaturgy +
                    conceptosNaturgy.EnergiaReactiva +
                    conceptosNaturgy.DerechosExtension +
                    conceptosNaturgy.DerechosAcceso +
                    impuestoNaturgy +
                    conceptosNaturgy.BonoSocial +
                    conceptosNaturgy.Descuentos +
                    conceptosNaturgy.AlquilerEquipos +
                    excedentesNaturgy;

                // PASO 9: Cálculo del IVA
                decimal ivaActual = baseActual * (reglas.PorcentajeIVA / 100);
                decimal ivaNaturgy = baseNaturgy * (reglas.PorcentajeIVA / 100);

                // PASO 10: Totales finales
                // El total actual se toma DIRECTAMENTE del reportedInvoiceTotal
                decimal totalActual = facturaActual.TotalReportado;
                decimal totalNaturgy = baseNaturgy + ivaNaturgy;

                // PASO 11: Cálculo del ahorro
                decimal ahorroPeriodo = totalActual - totalNaturgy;
                decimal ahorroDiario = ahorroPeriodo / dias;
                decimal ahorroAnual = ahorroDiario * 365;
                decimal ahorroMensual = ahorroAnual / 12;
                decimal porcentajeAhorro = totalActual > 0 ? (ahorroPeriodo / totalActual) * 100 : 0;

                // PASO 12: Determinar estado
                resultado.Status = ahorroPeriodo >= 0 ? "saving" : "overcost";
                if (ahorroPeriodo < 0)
                {
                    resultado.Warnings.Add("Esta oferta genera un sobrecoste estimado frente al total real de la factura actual.");
                }

                // Completar totales
                resultado.Totals = new TotalesComparativa
                {
                    TotalPowerCurrent = potenciaActual,
                    TotalPowerNaturgy = potenciaNaturgy,
                    TotalEnergyCurrent = energiaActual,
                    TotalEnergyNaturgy = energiaNaturgy,
                    TotalConsumption = facturaActual.Consumos.Sum(),
                    
                    ElectricityTaxBaseCurrent = baseImpuestoActual,
                    ElectricityTaxBaseNaturgy = baseImpuestoNaturgy,
                    
                    ElectricityTaxCurrent = impuestoActual,
                    ElectricityTaxNaturgy = impuestoNaturgy,
                    
                    TaxableBaseCurrent = baseActual,
                    TaxableBaseNaturgy = baseNaturgy,
                    
                    VatCurrent = ivaActual,
                    VatNaturgy = ivaNaturgy,
                    
                    TotalCurrent = totalActual,
                    TotalCurrentReal = totalActual,
                    TotalNaturgy = totalNaturgy,
                    
                    SavingPeriod = ahorroPeriodo,
                    SavingDaily = ahorroDiario,
                    SavingMonthly = ahorroMensual,
                    SavingAnnual = ahorroAnual,
                    SavingPercentCurrent = porcentajeAhorro,
                    LegacyPercent = totalNaturgy > 0 ? (totalActual / totalNaturgy) * 100 : 0
                };

                resultado.Concepts = new ConceptosComparativa
                {
                    Current = conceptosActual,
                    Naturgy = conceptosNaturgy
                };
            }
            catch (Exception ex)
            {
                resultado.Status = "error";
                resultado.Warnings.Add($"Error en el cálculo: {ex.Message}");
            }

            return resultado;
        }

        private (int potencia, int energia) ObtenerPeriodosPorPeaje(string peaje)
        {
            return peaje?.ToUpper() switch
            {
                "2.0TD" or "2.0" => (2, 3),
                "3.0TD" or "3.0" => (6, 6),
                "6.1TD" or "6.1" => (6, 6),
                _ => (2, 3) // Por defecto 2.0TD
            };
        }

        private int CalcularDias(DateOnly fechaInicio, DateOnly fechaFin)
        {
            return (fechaFin.ToDateTime(TimeOnly.MinValue) - fechaInicio.ToDateTime(TimeOnly.MinValue)).Days;
        }

        private (decimal actual, decimal naturgy, List<LineaDetalle> lineas) CalcularCostePotencia(
            DatosFacturaActual factura,
            DatosTarifaNaturgy tarifa,
            int periodos,
            int dias)
        {
            decimal totalActual = 0;
            decimal totalNaturgy = 0;
            var lineas = new List<LineaDetalle>();

            for (int i = 0; i < periodos; i++)
            {
                decimal potenciaContratada = factura.PotenciasContratadas.ElementAtOrDefault(i);
                decimal precioActual = factura.PreciosPotencia.ElementAtOrDefault(i);
                decimal precioNaturgy = tarifa.PreciosPotencia.ElementAtOrDefault(i);

                decimal costeActual = potenciaContratada * precioActual * dias;
                decimal costeNaturgy = potenciaContratada * precioNaturgy * dias;

                totalActual += costeActual;
                totalNaturgy += costeNaturgy;

                lineas.Add(new LineaDetalle
                {
                    Period = PERIODS[i],
                    Quantity = potenciaContratada,
                    CurrentPrice = precioActual,
                    NaturgyPrice = precioNaturgy,
                    CurrentAmount = costeActual,
                    NaturgyAmount = costeNaturgy
                });
            }

            return (totalActual, totalNaturgy, lineas);
        }

        private (decimal actual, decimal naturgy, List<LineaDetalle> lineas) CalcularCosteEnergia(
            DatosFacturaActual factura,
            DatosTarifaNaturgy tarifa,
            int periodos)
        {
            decimal totalActual = 0;
            decimal totalNaturgy = 0;
            var lineas = new List<LineaDetalle>();

            for (int i = 0; i < periodos; i++)
            {
                decimal consumo = factura.Consumos.ElementAtOrDefault(i);
                decimal precioActual = factura.PreciosEnergia.ElementAtOrDefault(i);
                decimal precioNaturgy = tarifa.PreciosEnergia.ElementAtOrDefault(i);

                decimal costeActual = consumo * precioActual;
                decimal costeNaturgy = consumo * precioNaturgy;

                totalActual += costeActual;
                totalNaturgy += costeNaturgy;

                lineas.Add(new LineaDetalle
                {
                    Period = PERIODS[i],
                    Quantity = consumo,
                    CurrentPrice = precioActual,
                    NaturgyPrice = precioNaturgy,
                    CurrentAmount = costeActual,
                    NaturgyAmount = costeNaturgy
                });
            }

            return (totalActual, totalNaturgy, lineas);
        }

        private ConceptosAdicionales AplicarReglasConceptos(ConceptosAdicionales actual, ReglasComparativa reglas)
        {
            return new ConceptosAdicionales
            {
                ExcesoPotencia = AplicarRegla(reglas.NaturgyExcessPower, actual.ExcesoPotencia),
                EnergiaReactiva = AplicarRegla(reglas.NaturgyReactive, actual.EnergiaReactiva),
                DerechosExtension = AplicarRegla(reglas.NaturgyExtensionRights, actual.DerechosExtension),
                DerechosAcceso = AplicarRegla(reglas.NaturgyAccessRights, actual.DerechosAcceso),
                BonoSocial = AplicarRegla(reglas.NaturgySocialBonus, actual.BonoSocial),
                Descuentos = AplicarRegla(reglas.NaturgyDiscounts, actual.Descuentos),
                AlquilerEquipos = AplicarRegla(reglas.NaturgyMeterRental, actual.AlquilerEquipos),
                PrecioExcedente = reglas.PrecioExcedentesNaturgy
            };
        }

        private decimal AplicarRegla(string regla, decimal valorActual)
        {
            if (regla == "zero") return 0;
            if (regla == "copy") return valorActual;
            if (regla.StartsWith("fixed:"))
            {
                if (decimal.TryParse(regla.Substring(6), out decimal valorFijo))
                    return valorFijo;
            }
            return valorActual;
        }
    }

    #region Modelos de datos

    public class DatosFacturaActual
    {
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public decimal TotalReportado { get; set; }
        
        public List<decimal> PotenciasContratadas { get; set; } = new();
        public List<decimal> PreciosPotencia { get; set; } = new();
        public List<decimal> Consumos { get; set; } = new();
        public List<decimal> PreciosEnergia { get; set; } = new();
        
        public decimal ExcedentesKwh { get; set; }
        public ConceptosAdicionales? Conceptos { get; set; }
    }

    public class DatosTarifaNaturgy
    {
        public string Familia { get; set; } = string.Empty;
        public string Variante { get; set; } = string.Empty;
        public string Peaje { get; set; } = string.Empty;
        public List<decimal> PreciosPotencia { get; set; } = new();
        public List<decimal> PreciosEnergia { get; set; } = new();
        public string Status { get; set; } = "valid";
        public int IdTarifa { get; set; }
        public string NombreTarifa { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
    }

    public class ConceptosAdicionales
    {
        public decimal ExcesoPotencia { get; set; }
        public decimal EnergiaReactiva { get; set; }
        public decimal DerechosExtension { get; set; }
        public decimal DerechosAcceso { get; set; }
        public decimal BonoSocial { get; set; }
        public decimal Descuentos { get; set; }
        public decimal AlquilerEquipos { get; set; }
        public decimal PrecioExcedente { get; set; }
    }

    public class ReglasComparativa
    {
        public decimal PorcentajeImpuestoElectrico { get; set; } = 5.11m;
        public decimal PorcentajeIVA { get; set; } = 21m;
        public decimal PrecioExcedentesNaturgy { get; set; } = 0.06m;
        
        public string NaturgyExcessPower { get; set; } = "copy";
        public string NaturgyReactive { get; set; } = "zero";
        public string NaturgyExtensionRights { get; set; } = "zero";
        public string NaturgyAccessRights { get; set; } = "zero";
        public string NaturgySocialBonus { get; set; } = "copy";
        public string NaturgyDiscounts { get; set; } = "zero";
        public string NaturgyMeterRental { get; set; } = "copy";

        public static ReglasComparativa Default() => new();
    }

    public class ResultadoCalculoComparativa
    {
        public int Days { get; set; }
        public int PowerPeriodCount { get; set; }
        public int EnergyPeriodCount { get; set; }
        public string Status { get; set; } = "saving";
        public List<string> Warnings { get; set; } = new();
        
        public List<LineaDetalle> PowerLines { get; set; } = new();
        public List<LineaDetalle> EnergyLines { get; set; } = new();
        public TotalesComparativa Totals { get; set; } = new();
        public ConceptosComparativa Concepts { get; set; } = new();
    }

    public class LineaDetalle
    {
        public string Period { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal NaturgyPrice { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal NaturgyAmount { get; set; }
    }

    public class TotalesComparativa
    {
        public decimal TotalPowerCurrent { get; set; }
        public decimal TotalPowerNaturgy { get; set; }
        public decimal TotalEnergyCurrent { get; set; }
        public decimal TotalEnergyNaturgy { get; set; }
        public decimal TotalConsumption { get; set; }
        
        public decimal ElectricityTaxBaseCurrent { get; set; }
        public decimal ElectricityTaxBaseNaturgy { get; set; }
        
        public decimal ElectricityTaxCurrent { get; set; }
        public decimal ElectricityTaxNaturgy { get; set; }
        
        public decimal TaxableBaseCurrent { get; set; }
        public decimal TaxableBaseNaturgy { get; set; }
        
        public decimal VatCurrent { get; set; }
        public decimal VatNaturgy { get; set; }
        
        public decimal TotalCurrent { get; set; }
        public decimal TotalCurrentReal { get; set; }
        public decimal TotalNaturgy { get; set; }
        
        public decimal SavingPeriod { get; set; }
        public decimal SavingDaily { get; set; }
        public decimal SavingMonthly { get; set; }
        public decimal SavingAnnual { get; set; }
        public decimal SavingPercentCurrent { get; set; }
        public decimal LegacyPercent { get; set; }
    }

    public class ConceptosComparativa
    {
        public ConceptosAdicionales Current { get; set; } = new();
        public ConceptosAdicionales Naturgy { get; set; } = new();
    }

    #endregion
}
