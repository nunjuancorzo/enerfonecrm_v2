using System;
using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using Xunit;

namespace EnerfoneCRM.Tests;

public class PlantillaEmailFirmaTests
{
    [Fact]
    public void UsaLogoInlinePaletaYEscapaLosDatosDelDestinatario()
    {
        var configuracion = new ConfiguracionEmpresa
        {
            NombreEmpresa = "Empresa <Central>",
            Email = "firma@example.com",
            LogoUrl = "data:image/png;base64," + Convert.ToBase64String([1, 2, 3]),
            TemaVisual = AparienciaService.PaletaPersonalizada,
            TemaColorAcento = "#123456",
            TemaColorBarra = "#234567",
            TemaColorFondo = "#F1F2F3"
        };

        var correo = PlantillaEmailFirma.Crear(
            configuracion,
            "Documentación pendiente",
            "<script>alert(1)</script>",
            "Revisa y firma la documentación.",
            "Consultar y firmar",
            "https://example.com/firma/token",
            DateTime.UtcNow.AddDays(2));

        Assert.NotNull(correo.Logo);
        Assert.Equal("image/png", correo.Logo.TipoMime);
        Assert.Contains("cid:logo-empresa", correo.Html);
        Assert.Contains("margin:0 auto", correo.Html);
        Assert.Contains("align=\"center\" bgcolor=\"#ffffff\"", correo.Html);
        Assert.Contains("#123456", correo.Html);
        Assert.Contains("#234567", correo.Html);
        Assert.Contains("#F1F2F3", correo.Html);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", correo.Html);
        Assert.DoesNotContain("<script>alert(1)</script>", correo.Html);
    }

    [Fact]
    public void ConservaUnaCabeceraDeMarcaSiNoHayImagen()
    {
        var correo = PlantillaEmailFirma.Crear(
            new ConfiguracionEmpresa { NombreEmpresa = "Enerfone" },
            "Firma pendiente",
            "Cliente",
            "Documento disponible.",
            "Firmar",
            "https://example.com/firma/token",
            DateTime.UtcNow.AddDays(2));

        Assert.Null(correo.Logo);
        Assert.Contains("Enerfone", correo.Html);
        Assert.Contains("Firmar", correo.Html);
    }
}