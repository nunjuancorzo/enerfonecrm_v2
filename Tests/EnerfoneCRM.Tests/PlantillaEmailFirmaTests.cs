using System;
using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using Xunit;

namespace EnerfoneCRM.Tests;

public class PlantillaEmailFirmaTests
{
    [Fact]
    public void UsaLaPaletaYEscapaLosDatosDelDestinatarioSinImagenIncrustada()
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

        var html = PlantillaEmailFirma.Crear(
            configuracion,
            "Documentación pendiente",
            "<script>alert(1)</script>",
            "Revisa y firma la documentación.",
            "Consultar y firmar",
            "https://example.com/firma/token",
            DateTime.UtcNow.AddDays(2));

        Assert.DoesNotContain("cid:", html);
        Assert.Contains("#123456", html);
        Assert.Contains("#234567", html);
        Assert.Contains("#F1F2F3", html);
        Assert.Contains("Empresa &lt;Central&gt;", html);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
        Assert.DoesNotContain("<script>alert(1)</script>", html);
    }

    [Fact]
    public void ConservaUnaCabeceraDeMarcaSiNoHayImagen()
    {
        var html = PlantillaEmailFirma.Crear(
            new ConfiguracionEmpresa { NombreEmpresa = "Enerfone" },
            "Firma pendiente",
            "Cliente",
            "Documento disponible.",
            "Firmar",
            "https://example.com/firma/token",
            DateTime.UtcNow.AddDays(2));

        Assert.Contains("Enerfone", html);
        Assert.Contains("Firmar", html);
    }

    [Fact]
    public void NotificacionNoIncluyeBotonNiCaducidadYEscapaLosDatos()
    {
        var html = PlantillaEmailFirma.CrearNotificacion(
            new ConfiguracionEmpresa { NombreEmpresa = "Enerfone", TemaColorAcento = "#123456" },
            "Nueva alta",
            "Administrador",
            "Se ha dado de alta un usuario.",
            [("Punto de venta", "<b>Bar & Co</b>")],
            "Se adjuntan los documentos.");

        Assert.DoesNotContain("<a href", html);
        Assert.DoesNotContain("caducidad", html);
        Assert.Contains("Punto de venta", html);
        Assert.Contains("&lt;b&gt;Bar &amp; Co&lt;/b&gt;", html);
        Assert.Contains("Se adjuntan los documentos.", html);
    }
}