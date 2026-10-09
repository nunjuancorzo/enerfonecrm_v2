using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using Xunit;

namespace EnerfoneCRM.Tests;

public class EmailNotificacionContratoTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("usuario", false)]
    [InlineData("usuario@", false)]
    [InlineData("usuario@@dominio.es", false)]
    [InlineData("usuario@dominio.es", true)]
    [InlineData(" usuario@dominio.es ", true)]
    public void EmailDeRegistroDebeEstarInformadoYSerValido(string? email, bool esperado)
    {
        Assert.Equal(esperado, EmailService.EsEmailValido(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/firma")]
    [InlineData("http://dominio.es")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://dominio.es?clave=valor")]
    public void ElCorreoDeFirmaNoPuedeContenerUnEnlaceRelativoOInseguro(string? baseUrl)
    {
        Assert.Throws<System.ArgumentException>(() => FirmaService.ConstruirUrlFirmaColaborador(baseUrl, "token"));
    }

    [Fact]
    public void ElEnlaceDeFirmaConservaLaRutaPublicaYEscapaElToken()
    {
        Assert.Equal("https://dominio.es/crm/firma-colaborador/token%2Fprueba", FirmaService.ConstruirUrlFirmaColaborador("https://dominio.es/crm/", "token/prueba"));
        Assert.Equal("http://localhost:5171/firma-colaborador/token", FirmaService.ConstruirUrlFirmaColaborador("http://localhost:5171", "token"));
    }

    [Fact]
    public void PymeIncluyeRazonSocialYRepresentanteEnAsuntoYCuerpo()
    {
        var cliente = new Cliente { TipoCliente = "Pyme", Nombre = "Representante de prueba", Empresa = "Empresa de prueba SL", Representante = "Representante de prueba" };
        Assert.Equal("Empresa de prueba SL - Representante: Representante de prueba", EmailService.ObtenerIdentificacionCliente(cliente));
        var cuerpo = EmailService.GenerarDatosClienteNotificacion(cliente);
        Assert.Contains("Empresa de prueba SL", cuerpo);
        Assert.Contains("Representante de prueba", cuerpo);
        Assert.Contains("Razón social", cuerpo);
    }

    [Fact]
    public void PymeUsaNombreComoRazonSocialCuandoEmpresaNoEstaInformada()
    {
        var cliente = new Cliente { TipoCliente = "Pyme", Nombre = "Empresa SL", Representante = "Persona representante" };
        Assert.Equal("Empresa SL - Representante: Persona representante", EmailService.ObtenerIdentificacionCliente(cliente));
    }

    [Fact]
    public void ConservaElRepresentanteDeLosDatosAntiguosEnNombre()
    {
        var cliente = new Cliente { TipoCliente = "Pyme", Nombre = "Persona representante", Empresa = "Empresa SL" };
        Assert.Equal("Empresa SL - Representante: Persona representante", EmailService.ObtenerIdentificacionCliente(cliente));
    }

    [Fact]
    public void NoInventaRepresentanteCuandoSoloConstaLaEmpresa()
    {
        var cliente = new Cliente { TipoCliente = "Pyme", Nombre = "Empresa SL" };
        Assert.Equal("Empresa SL", EmailService.ObtenerIdentificacionCliente(cliente));
        Assert.Contains("No informado", EmailService.GenerarDatosClienteNotificacion(cliente));
    }

    [Theory]
    [InlineData("Particular")]
    [InlineData("Autonomo")]
    public void MantieneLaIdentificacionDeClientesNoPyme(string tipo)
    {
        var cliente = new Cliente { TipoCliente = tipo, Nombre = "Cliente de prueba", Empresa = "Otra empresa", Representante = "Otra persona" };
        Assert.Equal("Cliente de prueba", EmailService.ObtenerIdentificacionCliente(cliente));
        Assert.DoesNotContain("Representante:", EmailService.GenerarDatosClienteNotificacion(cliente));
    }

    [Fact]
    public void EscapaHtmlEnLosNombres()
    {
        var cliente = new Cliente { TipoCliente = "Pyme", Nombre = "Empresa <SL>", Representante = "Persona & Socio" };
        var cuerpo = EmailService.GenerarDatosClienteNotificacion(cliente);
        Assert.Contains("Empresa &lt;SL&gt;", cuerpo);
        Assert.Contains("Persona &amp; Socio", cuerpo);
    }
}