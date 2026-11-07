using EnerfoneCRM.Services;
using Xunit;
using System;
using System.Text.Json;
using EnerfoneCRM.Models;

namespace EnerfoneCRM.Tests;

public class ImportacionDniServiceTests
{
    [Fact]
    public void ExtraeDniNombreApellidosYNacionalidad()
    {
        var datos = ImportacionDniService.ExtraerDatos("""
            DNI ESPAÑA
            APELLIDOS
            GARCÍA LÓPEZ
            NOMBRE
            ANA MARÍA
            DNI: 12345678Z
            NACIONALIDAD: ESP
            """);

        Assert.NotNull(datos);
        Assert.Equal("12345678Z", datos.Documento);
        Assert.Equal("DNI", datos.TipoDocumento);
        Assert.Equal("ANA MARÍA", datos.Nombre);
        Assert.Equal("GARCÍA LÓPEZ", datos.Apellidos);
        Assert.Equal("ANA MARÍA GARCÍA LÓPEZ", datos.NombreCompleto);
        Assert.Equal("Española", datos.Nacionalidad);
    }

    [Fact]
    public void ExtraeNieDesdeLaZonaDeLecturaMecanica()
    {
        var datos = ImportacionDniService.ExtraerDatos("IDESPX1234567L<<<<<<<<<<<<\nAPELLIDOS / SURNAME\nPÉREZ RUIZ\nNOMBRE / NAME\nLUIS");

        Assert.NotNull(datos);
        Assert.Equal("X1234567L", datos.Documento);
        Assert.Equal("NIE", datos.TipoDocumento);
    }

    [Fact]
    public void CorrigeSeparacionesYConfusionesOcrSoloCuandoElChecksumEsValido()
    {
        var datos = ImportacionDniService.ExtraerDatos("DNI: 1234 567BZ");

        Assert.NotNull(datos);
        Assert.Equal("12345678Z", datos.Documento);
        Assert.True(datos.CorregidoPorOcr);
    }

    [Fact]
    public void NoAceptaUnDocumentoConLetraDeControlIncorrecta()
    {
        var datos = ImportacionDniService.ExtraerDatos("DNI: 12345678A\nNOMBRE: ANA MARÍA\nAPELLIDOS: GARCÍA LÓPEZ");

        Assert.Null(datos);
    }

    [Fact]
    public void NoAceptaUnDocumentoConLongitudIncorrecta()
    {
        var datos = ImportacionDniService.ExtraerDatos("DNI: 1234567T");

        Assert.Null(datos);
    }

    [Fact]
    public void NoConfundeUnaFacturaConUnDocumentoDeIdentidad()
    {
        var datos = ImportacionDniService.ExtraerDatos("Titular: ANA MARÍA GARCÍA\nCUPS: ES1234567890123456AB\nTotal: 12345678 EUR");

        Assert.Null(datos);
    }

    [Fact]
    public void GeminiValidaDocumentoYSeparaDatosSinGuardarCliente()
    {
        var datos = GeminiDniService.Interpretar("{\"Documento\":\"12345678Z\",\"TipoDocumento\":\"DNI\",\"Nombre\":\"Ana\",\"Apellidos\":\"Garcia\",\"Nacionalidad\":\"ESP\",\"Direccion\":\"Calle Prueba\",\"CodigoPostal\":\"28001\"}");
        Assert.Equal("12345678Z", datos.Documento);
        Assert.Equal("Ana Garcia", datos.NombreCompleto);
        Assert.Equal("Española", datos.Nacionalidad);
        Assert.Equal("28001", datos.CodigoPostal);
    }

    [Theory]
    [InlineData("12345678A", "DNI")]
    [InlineData("12345678Z", "NIE")]
    [InlineData("X1234567L", "DNI")]
    public void GeminiRechazaChecksumOTipoIncorrecto(string documento, string tipo)
    {
        var json = JsonSerializer.Serialize(new { Documento = documento, TipoDocumento = tipo });
        Assert.Throws<ArgumentException>(() => GeminiDniService.Interpretar(json));
    }

    [Fact]
    public void GeminiNoAdmiteCamposFueraDelDocumento()
    {
        Assert.Throws<JsonException>(() => GeminiDniService.Interpretar("{\"Documento\":\"12345678Z\",\"TipoDocumento\":\"DNI\",\"EmpresaId\":2}"));
    }

    [Fact]
    public void GeminiExtraeElDomicilioDesglosado()
    {
        var datos = GeminiDniService.Interpretar(JsonSerializer.Serialize(new
        {
            Documento = "12345678Z", TipoDocumento = "DNI", TipoVia = "Avda.", Direccion = "De la Prueba", Numero = "12 bis",
            Escalera = "B", Piso = "3", Puerta = "Izq.", CodigoPostal = "28001", Poblacion = "Madrid", Provincia = "Madrid",
            Pais = "Espana", Aclarador = "Bloque 2"
        }));
        Assert.Equal("AVENIDA", datos.TipoVia);
        Assert.Equal("De la Prueba", datos.Direccion);
        Assert.Equal("12 bis", datos.Numero);
        Assert.Equal("B", datos.Escalera);
        Assert.Equal("3", datos.Piso);
        Assert.Equal("Izq.", datos.Puerta);
        Assert.Equal("28001", datos.CodigoPostal);
        Assert.Equal("Madrid", datos.Poblacion);
        Assert.Equal("Madrid", datos.Provincia);
        Assert.Equal("Bloque 2", datos.Aclarador);
    }

    [Fact]
    public void GeminiNoInventaDireccionNiCodigoPostalSiNoAparecen()
    {
        var datos = GeminiDniService.Interpretar("{\"Documento\":\"12345678Z\",\"TipoDocumento\":\"DNI\"}");
        Assert.Null(datos.Direccion);
        Assert.Null(datos.CodigoPostal);
        Assert.Null(datos.Poblacion);
        Assert.Null(datos.Provincia);
    }

    [Fact]
    public void AplicaLaDireccionDesglosadaALosCamposDelCliente()
    {
        var datos = GeminiDniService.Interpretar(JsonSerializer.Serialize(new
        {
            Documento = "12345678Z", TipoDocumento = "DNI", Nombre = "Ana", Apellidos = "Garcia", TipoVia = "C/",
            Direccion = "Mayor", Numero = "12", Escalera = "B", Piso = "3", Puerta = "A", CodigoPostal = "28001",
            Poblacion = "Madrid", Provincia = "Madrid", Pais = "Espana", Aclarador = "Bloque 2"
        }));
        var cliente = new Cliente();
        GeminiDniService.AplicarAlCliente(cliente, datos);
        Assert.Equal("CALLE", cliente.TipoVia);
        Assert.Equal("Mayor", cliente.Direccion);
        Assert.Equal("12", cliente.Numero);
        Assert.Equal("B", cliente.Escalera);
        Assert.Equal("3", cliente.Piso);
        Assert.Equal("A", cliente.Puerta);
        Assert.Equal("28001", cliente.CodigoPostal);
        Assert.Equal("Madrid", cliente.Poblacion);
        Assert.Equal("Madrid", cliente.Ciudad);
        Assert.Equal("Madrid", cliente.Provincia);
        Assert.Equal("Espana", cliente.Pais);
        Assert.Equal("Bloque 2", cliente.Aclarador);
    }

    [Fact]
    public void NoBorraCamposExistentesQueNoSeDetectaronEnElDni()
    {
        var cliente = new Cliente { Direccion = "Mayor", Numero = "12", CodigoPostal = "28001", Poblacion = "Madrid", Provincia = "Madrid" };
        var datos = GeminiDniService.Interpretar("{\"Documento\":\"12345678Z\",\"TipoDocumento\":\"DNI\"}");
        GeminiDniService.AplicarAlCliente(cliente, datos);
        Assert.Equal("Mayor", cliente.Direccion);
        Assert.Equal("12", cliente.Numero);
        Assert.Equal("28001", cliente.CodigoPostal);
        Assert.Equal("Madrid", cliente.Poblacion);
        Assert.Equal("Madrid", cliente.Provincia);
    }

    [Fact]
    public void NoAplicaCambiosParcialesSiLaRevisionTieneUnCodigoPostalInvalido()
    {
        var cliente = new Cliente { Nombre = "Original", Direccion = "Anterior" };
        var datos = new DatosDocumentoIdentidad("12345678Z", "DNI", "Ana", "Garcia", null, "Nueva", "28A01");
        Assert.Throws<ArgumentException>(() => GeminiDniService.AplicarAlCliente(cliente, datos));
        Assert.Equal("Original", cliente.Nombre);
        Assert.Equal("Anterior", cliente.Direccion);
    }
}