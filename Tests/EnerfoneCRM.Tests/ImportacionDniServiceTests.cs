using EnerfoneCRM.Services;
using Xunit;

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
}