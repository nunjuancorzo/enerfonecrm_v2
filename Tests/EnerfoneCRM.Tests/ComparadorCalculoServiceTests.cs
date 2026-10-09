using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using Xunit;

namespace EnerfoneCRM.Tests;

public class ComparadorCalculoServiceTests
{
    private readonly ComparadorCalculoService servicio = new();

    [Theory]
    [InlineData("0,125000", "0.125")]
    [InlineData(" 0.125000 ", "0.125")]
    [InlineData("0", "0")]
    public void ConviertePreciosSinDependerDeLaCultura(string texto, string esperado)
    {
        Assert.Equal(decimal.Parse(esperado, CultureInfo.InvariantCulture), ComparadorCalculoService.LeerPrecioTarifa(texto, "Precio"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Consultar")]
    [InlineData("-0.1")]
    [InlineData("1.234,56")]
    public void RechazaPreciosAusentesNegativosOAmbiguos(string? precio)
    {
        Assert.Throws<ArgumentException>(() => ComparadorCalculoService.LeerPrecioTarifa(precio, "Precio"));
    }

    [Theory]
    [InlineData("3.0")]
    [InlineData("3.0TD")]
    [InlineData(" 6.1td ")]
    public void UsaLosSeisPeriodosDeTarifasLuz(string peaje)
    {
        var datos = ComparadorCalculoService.PrepararTarifaLuz(CrearTarifaLuz(peaje));
        var factura = new DatosFacturaActual
        {
            FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 1, 31), TotalReportado = 100,
            PotenciasContratadas = new() { 2, 2, 2, 2, 2, 2 }, Consumos = new() { 10, 10, 10, 10, 10, 10 }
        };
        var resultado = servicio.CalcularComparativa(factura, datos, new ReglasComparativa { PorcentajeIVA = 0, PorcentajeImpuestoElectrico = 0 });

        Assert.Equal(6, datos.PreciosPotencia.Count);
        Assert.Equal(6, datos.PreciosEnergia.Count);
        Assert.Equal(3.6m, resultado.PowerLines[5].NaturgyAmount);
        Assert.Equal(6m, resultado.EnergyLines[5].NaturgyAmount);
        Assert.Equal(33.6m, resultado.Totals.TotalNaturgy);
        Assert.Equal(66.4m, resultado.Totals.SavingPeriod);
    }

    [Fact]
    public void Tarifa20UtilizaDosPotenciasYTresEnergias()
    {
        var tarifa = CrearTarifaLuz("2.0TD");
        tarifa.Potencia6 = null;
        tarifa.Energia6 = "Consultar";
        var datos = ComparadorCalculoService.PrepararTarifaLuz(tarifa);
        Assert.Equal(new List<decimal> { 0.01m, 0.02m }, datos.PreciosPotencia);
        Assert.Equal(new List<decimal> { 0.1m, 0.2m, 0.3m }, datos.PreciosEnergia);
        Assert.Equal(tarifa.Id, datos.IdTarifa);
        Assert.Equal(tarifa.Empresa, datos.Empresa);
    }

    [Fact]
    public void NoSustituyePeriodosRequeridosPorCeros()
    {
        var tarifa = CrearTarifaLuz("3.0TD");
        tarifa.Energia4 = null;
        Assert.Throws<ArgumentException>(() => ComparadorCalculoService.PrepararTarifaLuz(tarifa));
    }

    [Fact]
    public void RechazaPeajesElectricosDesconocidos()
    {
        Assert.Throws<ArgumentException>(() => ComparadorCalculoService.PrepararTarifaLuz(CrearTarifaLuz("RL.1")));
    }

    [Fact]
    public void GasIncluyeFijoVariableHidrocarburosContadorOtrosEIva()
    {
        var factura = CrearFacturaGas();
        var resultado = servicio.CalcularComparativaGas(factura, new TarifaGas { TerminoFijoGas = "0,2", TerminoVariableGas = "0.05" });
        Assert.Equal(6m, resultado.Totals.TotalFixedNaturgy);
        Assert.Equal(50m, resultado.Totals.TotalEnergyNaturgy);
        Assert.Equal(2.34m, resultado.Totals.HydrocarbonTaxNaturgy);
        Assert.Equal(75.4314m, resultado.Totals.TotalNaturgy);
        Assert.Equal(24.5686m, resultado.Totals.SavingPeriod);
        Assert.Equal(0m, resultado.Totals.TotalPowerNaturgy);
        Assert.Equal(0m, resultado.Totals.ElectricityTaxNaturgy);
    }

    [Fact]
    public void GasConvierteUnFijoMensualSinMultiplicarloPorTreinta()
    {
        var factura = CrearFacturaGas();
        factura.TerminoFijoMensual = true;
        var resultado = servicio.CalcularComparativaGas(factura, new TarifaGas { TerminoFijoGas = "5", TerminoVariableGas = "0.05" });
        var fijoMensualEstimado = resultado.Totals.TotalFixedNaturgy * 365m / (resultado.Days * 12m);
        Assert.InRange(fijoMensualEstimado, 4.999999999999m, 5.000000000001m);
    }

    [Fact]
    public void GasRechazaFechasSinDuracion()
    {
        var factura = CrearFacturaGas();
        factura.FechaFin = factura.FechaInicio;
        Assert.Throws<ArgumentException>(() => servicio.CalcularComparativaGas(factura, new TarifaGas { TerminoFijoGas = "5", TerminoVariableGas = "0.05" }));
    }

    [Fact]
    public void GasRechazaTerminosAusentes()
    {
        Assert.Throws<ArgumentException>(() => servicio.CalcularComparativaGas(CrearFacturaGas(), new TarifaGas { TerminoFijoGas = "5" }));
    }

    [Theory]
    [InlineData("Luz")]
    [InlineData("Gas")]
    public void GeneraPdfParaAmbosSuministros(string suministro)
    {
        var datos = new PdfComparadorService.DatosComparativa
        {
            TipoSuministro = suministro,
            FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 1, 31),
            NombreCliente = "Cliente de prueba", CompaniaActual = "Compania actual", TarifaActual = suministro == "Gas" ? "RL.1" : "3.0TD",
            PotenciasContratadas = suministro == "Gas" ? new() : new() { 2, 2, 2, 2, 2, 2 },
            Notas = "Estimacion con precios base. Descuentos y servicios adicionales no incluidos.",
            Ofertas = new()
            {
                new PdfComparadorService.OfertaComparativa
                {
                    Posicion = 1, Empresa = "Compania nueva", NombreTarifa = "Tarifa nueva",
                    CosteActual = 100, CosteMensual = 75, AhorroMensual = 25, AhorroAnual = 300
                }
            }
        };
        var pdf = new PdfComparadorService().GenerarComparativaPdf(datos);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    private static TarifaLuz CrearTarifaLuz(string peaje) => new()
    {
        Id = 42, Empresa = "Empresa de prueba", Nombre = "Tarifa nueva", Peaje = peaje,
        Potencia1 = "0,01", Potencia2 = "0.02", Potencia3 = "0.03", Potencia4 = "0.04", Potencia5 = "0.05", Potencia6 = "0.06",
        Energia1 = "0.1", Energia2 = "0.2", Energia3 = "0.3", Energia4 = "0.4", Energia5 = "0.5", Energia6 = "0.6"
    };

    private static DatosFacturaGas CrearFacturaGas() => new()
    {
        FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 1, 31), TotalReportado = 100,
        ConsumoKwh = 1000, PorcentajeIVA = 21, ImpuestoHidrocarburosPorKwh = 0.00234m, AlquilerEquipos = 1, OtrosConceptos = 3
    };
}