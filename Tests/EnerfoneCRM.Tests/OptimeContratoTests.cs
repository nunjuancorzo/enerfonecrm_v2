using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Xunit;
using PdfLectura = UglyToad.PdfPig.PdfDocument;

namespace EnerfoneCRM.Tests;

public class OptimeContratoTests
{
    private static readonly DateTime Fecha = new(2026, 10, 15, 11, 30, 0);
    private static readonly byte[] Firma = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aX1sAAAAASUVORK5CYII=");

    [Fact]
    public void NuevoContratoRellenaSoloDatosOriginales()
    {
        var solicitud = Solicitud();
        var bytes = OptimeFirmaService.GenerarDocumento(OptimeFirma.TipoContrato, solicitud, Fecha);
        using var pdf = PdfLectura.Open(bytes);
        Assert.Equal(25, pdf.NumberOfPages);
        var pagina = pdf.GetPage(1);
        var texto = string.Join(" ", pagina.GetWords().Select(w => w.Text));
        Assert.Contains("Enrique Barandiaran Reus", texto);
        Assert.Contains(solicitud.PersonaContacto, texto);
        Assert.Contains(solicitud.NifPersonaContacto, texto);
        Assert.Contains(solicitud.Email, texto);
        Assert.Contains("octubre", texto);
        Assert.DoesNotContain("Luis Garzón", texto);
        Assert.Contains(solicitud.RazonSocial, texto);
        Assert.Contains(solicitud.CifNif, texto);
        Assert.Contains(solicitud.DireccionFiscal, texto);
        Assert.DoesNotContain(pagina.GetWords(), w =>
            w.Text == "X" && w.BoundingBox.Left >= 96 && w.BoundingBox.Left <= 105 &&
            842 - w.BoundingBox.Bottom >= 385 && 842 - w.BoundingBox.Bottom <= 452);
        AssertCamposRepresentacionVacios(pagina);
    }

    [Fact]
    public void FirmaNuevoContratoIncluyeTresFirmasSinTaparNumeracionNiTextoOriginal()
    {
        var solicitud = Solicitud();
        var original = OptimeFirmaService.GenerarDocumento(OptimeFirma.TipoContrato, solicitud, Fecha);
        var firmado = OptimeFirmaService.FirmarDocumento(OptimeFirma.TipoContrato, original, solicitud, Firma, "203.0.113.10", Fecha);
        using var pdfOriginal = PdfLectura.Open(original);
        using var pdfFirmado = PdfLectura.Open(firmado);
        Assert.Equal(25, pdfFirmado.NumberOfPages);
        AssertCamposRepresentacionVacios(pdfFirmado.GetPage(1));
        for (var numero = 1; numero <= 25; numero++)
        {
            var pagina = pdfFirmado.GetPage(numero);
            var paginaOriginal = pdfOriginal.GetPage(numero);
            var palabras = pagina.GetWords().Select(w => w.Text).ToArray();
            Assert.All(paginaOriginal.GetWords(), w => Assert.Contains(w.Text, palabras));
            Assert.Contains("203.0.113.10", palabras);
            var firmas = numero is 12 or 18 or 25 ? 2 : 1;
            Assert.Equal(paginaOriginal.GetImages().Count() + firmas, pagina.GetImages().Count());
            var pie = pagina.GetWords().Where(w => w.Text == "IP" && w.BoundingBox.Top < 30).Single();
            Assert.True(pie.BoundingBox.Top < 30);
        }
        foreach (var (numero, arriba, abajo) in new[] { (12, 407d, 485d), (18, 168d, 245d), (25, 466d, 544d) })
        {
            var firma = pdfFirmado.GetPage(numero).GetImages()
                .Single(i => i.BoundingBox.Left >= 290 && i.BoundingBox.Top > 100);
            Assert.True(842 - firma.BoundingBox.Top >= arriba);
            Assert.True(842 - firma.BoundingBox.Bottom <= abajo);
            Assert.True(firma.BoundingBox.Right <= 460);
        }
    }

    [Fact]
    public void FirmaPendienteConservaContratoAnteriorAunqueCambieLaPlantillaOLosDatos()
    {
        using var doc = new PdfDocument();
        for (var i = 0; i < 19; i++)
        {
            var pagina = doc.AddPage();
            using var g = XGraphics.FromPdfPage(pagina);
            g.DrawString("CONTRATO ORIGINAL PENDIENTE", new XFont("Arial", 12), XBrushes.Black, new XPoint(50, 80));
        }
        using var stream = new MemoryStream();
        doc.Save(stream, false);
        var solicitud = Solicitud();
        var firmado = OptimeFirmaService.FirmarDocumento(OptimeFirma.TipoContrato, stream.ToArray(), solicitud, Firma, null, Fecha);
        using var pdf = PdfLectura.Open(firmado);
        Assert.Equal(19, pdf.NumberOfPages);
        for (var numero = 1; numero <= 19; numero++)
        {
            Assert.Contains("CONTRATO ORIGINAL PENDIENTE", string.Join(" ", pdf.GetPage(numero).GetWords().Select(w => w.Text)));
            Assert.Equal(numero is 9 or 19 ? 2 : 1, pdf.GetPage(numero).GetImages().Count());
        }
    }

    [Theory]
    [InlineData(OptimeFirma.TipoAdenda, 3)]
    [InlineData(OptimeFirma.TipoCodigoBuenasPracticas, 1)]
    public void OtrosDocumentosConservanPlantillaYFirma(string tipo, int paginaFirma)
    {
        var solicitud = Solicitud();
        var original = OptimeFirmaService.GenerarDocumento(tipo, solicitud, Fecha);
        var firmado = OptimeFirmaService.FirmarDocumento(tipo, original, solicitud, Firma, null, Fecha);
        using var pdfOriginal = PdfLectura.Open(original);
        using var pdfFirmado = PdfLectura.Open(firmado);
        Assert.Equal(pdfOriginal.NumberOfPages, pdfFirmado.NumberOfPages);
        var pagina = pdfFirmado.GetPage(paginaFirma);
        Assert.Equal(pdfOriginal.GetPage(paginaFirma).GetImages().Count() + 2, pagina.GetImages().Count());
    }

    [Fact]
    public void FichaValidaNoNecesitaCamposAdicionales()
    {
        var solicitud = Solicitud();
        var errores = new List<ValidationResult>();
        Assert.True(Validator.TryValidateObject(solicitud, new ValidationContext(solicitud), errores, true));
        Assert.Empty(errores);
        Assert.Null(typeof(OptimeSolicitud).GetProperty("TipoColaborador"));
        Assert.Null(typeof(OptimeSolicitud).GetProperty("CargoRepresentante"));
        Assert.Null(typeof(OptimeSolicitud).GetProperty("AcreditacionRepresentacion"));
    }

    [Fact]
    public void DatosPersonalesExistentesNoNecesitanClasificacion()
    {
        var solicitud = Solicitud();
        solicitud.RazonSocial = solicitud.PersonaContacto;
        solicitud.CifNif = solicitud.NifPersonaContacto;
        Assert.True(Validator.TryValidateObject(solicitud, new ValidationContext(solicitud), new List<ValidationResult>(), true));
        using var pdf = PdfLectura.Open(OptimeFirmaService.GenerarDocumento(OptimeFirma.TipoContrato, solicitud, Fecha));
        Assert.Equal(25, pdf.NumberOfPages);
        AssertCamposRepresentacionVacios(pdf.GetPage(1));
    }

    [Fact]
    public void ContratoSigueValidandoDatosOriginalesObligatorios()
    {
        var solicitud = Solicitud();
        solicitud.Email = string.Empty;
        Assert.Throws<ValidationException>(() => OptimeFirmaService.GenerarDocumento(OptimeFirma.TipoContrato, solicitud, Fecha));
    }

    [Fact]
    public void CampoDemasiadoLargoNoInvadeTextoLegal()
    {
        var solicitud = Solicitud();
        solicitud.RazonSocial = new string('W', 200);
        Assert.Throws<ValidationException>(() => OptimeFirmaService.GenerarDocumento(OptimeFirma.TipoContrato, solicitud, Fecha));
    }

    private static void AssertCamposRepresentacionVacios(UglyToad.PdfPig.Content.Page pagina)
    {
        Assert.DoesNotContain(pagina.GetWords(), w =>
            w.BoundingBox.Left >= 176 && w.BoundingBox.Right <= 284 &&
            842 - w.BoundingBox.Bottom >= 489 && 842 - w.BoundingBox.Bottom <= 504);
        Assert.DoesNotContain(pagina.GetWords(), w =>
            842 - w.BoundingBox.Bottom >= 507 && 842 - w.BoundingBox.Bottom <= 550);
    }

    private static OptimeSolicitud Solicitud() => new()
    {
        RazonSocial = "Empresa Ejemplo SL",
        CifNif = "B12345678",
        PersonaContacto = "Ana Perez Garcia",
        NifPersonaContacto = "12345678Z",
        DireccionFiscal = "Calle Ejemplo 15",
        CodigoPostal = "41015",
        Poblacion = "Sevilla",
        Provincia = "Sevilla",
        Email = "ana@example.test",
        Telefono = "600123456"
    };
}
