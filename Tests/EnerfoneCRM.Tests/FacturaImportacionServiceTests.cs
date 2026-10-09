using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EnerfoneCRM.Services;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using Xunit;

namespace EnerfoneCRM.Tests;

public class FacturaImportacionServiceTests
{
    [Fact]
    public void ResumenNoInventaPotenciasNiReparteElConsumo()
    {
        var factura = FacturaImportacionService.AnalizarTexto("""
            Informacion del consumo electrico
            Periodo de facturacion: del 31/05/2021 a 22/06/2021 (22 dias)
            Total 32,74 EUR
            Consumo total 129 kWh
            En esta factura el consumo ha salido a 0,1078 EUR/kWh
            """);
        Assert.Equal("2021-05-31", factura.Obtener("fecha_inicio"));
        Assert.Equal("2021-06-22", factura.Obtener("fecha_fin"));
        Assert.Equal("32.74", factura.Obtener("total_factura"));
        Assert.Equal("129", factura.Obtener("consumo_total"));
        Assert.Null(factura.Obtener("potencia_p1"));
        Assert.Null(factura.Obtener("consumo_p1"));
        Assert.Null(factura.Obtener("precio_energia_p1"));
        Assert.Contains(factura.Advertencias, aviso => aviso.Contains("no se repartira", StringComparison.OrdinalIgnoreCase) || aviso.Contains("no se repartirá"));
    }

    [Fact]
    public void IdentificaUnaCompaniaNoIncluidaEnPlantillasYLosSeisPeriodos()
    {
        var factura = FacturaImportacionService.AnalizarTexto("""
            Comercializadora: Empresa Nueva S.A.
            Titular: Cliente de prueba
            Peaje de acceso 3.0TD
            Periodo facturado 1 de enero de 2026 hasta 31 de enero de 2026
            Importe a pagar: 1.234,56 EUR
            Potencia P4: 12,5 kW
            P6 55 kWh x 0,125 EUR/kWh = 6,875 EUR
            Precio potencia P4: 0,123 EUR/kW/dia
            IVA 21%
            """);
        Assert.Equal("Empresa Nueva S.A.", factura.Obtener("comercializadora_actual"));
        Assert.Equal("3.0TD", factura.Obtener("peaje_luz"));
        Assert.Equal("2026-01-01", factura.Obtener("fecha_inicio"));
        Assert.Equal("1234.56", factura.Obtener("total_factura"));
        Assert.Equal("12.5", factura.Obtener("potencia_p4"));
        Assert.Equal("55", factura.Obtener("consumo_p6"));
        Assert.Equal("0.125", factura.Obtener("precio_energia_p6"));
        Assert.Equal("0.123", factura.Obtener("precio_potencia_p4"));
        Assert.Equal("dia", factura.Obtener("unidad_potencia"));
        Assert.Equal("21", factura.Obtener("iva_porcentaje"));
    }

    [Fact]
    public void ExtraeGasSinConvertirElFijoActualEnLaUnidadDelCatalogo()
    {
        var factura = FacturaImportacionService.AnalizarTexto("""
            Proveedor: Gas Nueva Compania
            Peaje RL.2
            Fecha inicio: 01/02/2026
            Fecha fin: 01/03/2026
            Total factura: 120,50 EUR
            Consumo de gas: 850 kWh
            Termino fijo gas: 5,50 EUR/mes
            Termino variable gas: 0,06 EUR/kWh
            """);
        Assert.Equal("Gas", factura.Obtener("tipo_suministro"));
        Assert.Equal("RL.2", factura.Obtener("peaje_gas"));
        Assert.Equal("850", factura.Obtener("consumo_total"));
        Assert.Equal("120.50", factura.Obtener("total_factura"));
        Assert.Null(factura.Obtener("termino_fijo_gas"));
        Assert.Equal("5.50", factura.Campos.Single(campo => campo.Clave == "termino_fijo_gas").Valor);
    }

    [Fact]
    public void NoConfundeIbanLecturasNiPrecioConPotenciaContratada()
    {
        var factura = FacturaImportacionService.AnalizarTexto("""
            IBAN ES1234567890123456789012
            Lectura contador P1 345,67 kWh
            Precio potencia P1: 0,12 EUR/kW/dia
            IVA 4,40 EUR
            """);
        Assert.Null(factura.Obtener("cups"));
        Assert.Null(factura.Obtener("consumo_p1"));
        Assert.Null(factura.Obtener("potencia_p1"));
        Assert.Null(factura.Obtener("iva_porcentaje"));
    }

    [Fact]
    public void LosTotalesContradictoriosNoSeSeleccionanAunqueElPrimeroSeRepita()
    {
        var factura = FacturaImportacionService.AnalizarTexto("Total factura: 32,74 EUR\nTotal factura: 75,10 EUR\nTotal factura: 32,74 EUR");
        Assert.Null(factura.Obtener("total_factura"));
        Assert.True(factura.Campos.Single(campo => campo.Clave == "total_factura").Conflicto);
    }

    [Fact]
    public void UnaFacturaMixtaRequiereSepararTotalYConsumo()
    {
        var factura = FacturaImportacionService.AnalizarTexto("Peaje 2.0TD\nPeaje RL.1\nTotal factura: 300,00 EUR\nConsumo total: 1500 kWh");
        Assert.Equal("Mixto", factura.Obtener("tipo_suministro"));
        Assert.Null(factura.Obtener("total_factura"));
        Assert.Null(factura.Obtener("consumo_total"));
        Assert.NotEmpty(FacturaImportacionService.ValidarParaAplicar(factura));
    }

    [Theory]
    [InlineData("32,74", 32.74)]
    [InlineData("1234.56", 1234.56)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("1,234.56", 1234.56)]
    public void NormalizaImportesDeVariosFormatos(string texto, double esperado)
    {
        Assert.True(FacturaImportacionService.TryNumero(texto, out var valor));
        Assert.Equal((decimal)esperado, valor);
    }

    [Fact]
    public void UnNumeroAmbiguoRequiereConfirmacion()
    {
        Assert.False(FacturaImportacionService.TryNumero("1.234", out _));
    }

    [Fact]
    public void LaUnidadDePotenciaDebeConfirmarseAntesDeAplicarPrecios()
    {
        var factura = FacturaImportacionService.AnalizarTexto("Peaje 2.0TD");
        FacturaImportacionService.IncorporarDatos(factura, new System.Collections.Generic.Dictionary<string, string> { ["precio_potencia_p1"] = "0.12" }, "Prueba");
        Assert.Contains(FacturaImportacionService.ValidarParaAplicar(factura), error => error.Contains("unidad"));
    }

    [Fact]
    public void EnviaPdfComoArchivoYNoComoJpeg()
    {
        var contenido = JsonSerializer.SerializeToElement(OcrService.CrearContenidoDocumento(Encoding.ASCII.GetBytes("%PDF-1.7"), "factura.pdf"));
        Assert.Equal("file", contenido.GetProperty("type").GetString());
        Assert.StartsWith("data:application/pdf;base64,", contenido.GetProperty("file").GetProperty("file_data").GetString());
    }

    [Fact]
    public void VerificaElFormatoRealDelArchivo()
    {
        Assert.Equal("image/png", OcrService.ObtenerTipoDocumento(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.Throws<ArgumentException>(() => OcrService.ObtenerTipoDocumento(Encoding.ASCII.GetBytes("archivo falso.pdf")));
    }

    [Fact]
    public async Task ExtraeUnPdfDigitalSinEnviarDatosFueraDelServidor()
    {
        var clientes = new SinLlamadasExternas();
        var servicio = new FacturaImportacionService(new OcrService(null!, clientes));
        var factura = await servicio.ExtraerAsync(CrearPdf(), "factura.pdf");
        Assert.Equal("PDF local", factura.Origen);
        Assert.Equal("54.21", factura.Obtener("total_factura"));
        Assert.Equal("210", factura.Obtener("consumo_total"));
        Assert.Equal(0, clientes.Llamadas);
    }

    [Fact]
    public async Task RechazaUnPdfConDemasiadasPaginasAntesDeHacerOcr()
    {
        var clientes = new SinLlamadasExternas();
        var servicio = new FacturaImportacionService(new OcrService(null!, clientes));
        await Assert.ThrowsAsync<ArgumentException>(() => servicio.ExtraerAsync(CrearPdf(21), "factura.pdf"));
        Assert.Equal(0, clientes.Llamadas);
    }

    [Fact]
    public async Task RechazaArchivosExcesivosOVacios()
    {
        var servicio = new FacturaImportacionService(new OcrService(null!, new SinLlamadasExternas()));
        await Assert.ThrowsAsync<ArgumentException>(() => servicio.ExtraerAsync(Array.Empty<byte>(), "factura.pdf"));
        await Assert.ThrowsAsync<ArgumentException>(() => servicio.ExtraerAsync(new byte[FacturaImportacionService.MaximoBytes + 1], "factura.pdf"));
    }

    [Fact]
    [Trait("Category", "OCRLocal")]
    public async Task LeeUnaImagenEscaneadaConOcrLocalSinLlamadasExternas()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "factura-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(carpeta);
        try
        {
            var pdf = Path.Combine(carpeta, "factura.pdf");
            await File.WriteAllBytesAsync(pdf, CrearPdf());
            using var conversion = new Process { StartInfo = new ProcessStartInfo("pdftoppm") { UseShellExecute = false, RedirectStandardError = true } };
            foreach (var argumento in new[] { "-singlefile", "-r", "150", "-png", pdf, Path.Combine(carpeta, "imagen") }) conversion.StartInfo.ArgumentList.Add(argumento);
            conversion.Start();
            await conversion.WaitForExitAsync();
            Assert.Equal(0, conversion.ExitCode);
            var clientes = new SinLlamadasExternas();
            var servicio = new FacturaImportacionService(new OcrService(null!, clientes));
            var factura = await servicio.ExtraerAsync(await File.ReadAllBytesAsync(Path.Combine(carpeta, "imagen.png")), "nombre-incorrecto.pdf");
            Assert.Equal("OCR local", factura.Origen);
            Assert.Equal("54.21", factura.Obtener("total_factura"));
            Assert.Equal("210", factura.Obtener("consumo_total"));
            Assert.Equal(0, clientes.Llamadas);
        }
        finally { Directory.Delete(carpeta, true); }
    }

    private static byte[] CrearPdf(int paginas = 1)
    {
        var builder = new PdfDocumentBuilder();
        var fuente = builder.AddStandard14Font(Standard14Font.Helvetica);
        var lineas = new[] { "Comercializadora: Energia de Prueba", "Peaje 2.0TD", "Periodo facturado 01/01/2026 a 31/01/2026", "Total a pagar: 54,21 EUR", "Consumo total: 210 kWh" };
        for (var numeroPagina = 0; numeroPagina < paginas; numeroPagina++)
        {
            var pagina = builder.AddPage(UglyToad.PdfPig.Content.PageSize.A4);
            for (var indice = 0; indice < lineas.Length; indice++) pagina.AddText(lineas[indice], 16, new PdfPoint(35, 740 - indice * 45), fuente);
        }
        return builder.Build();
    }

    private class SinLlamadasExternas : IHttpClientFactory
    {
        public int Llamadas { get; private set; }
        public HttpClient CreateClient(string name) { Llamadas++; throw new InvalidOperationException("No debe enviarse la factura a un servicio externo."); }
    }
}