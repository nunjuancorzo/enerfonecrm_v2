using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using EnerfoneCRM.Components.Layout;
using EnerfoneCRM.Components.Pages;
using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Xunit;

namespace EnerfoneCRM.Tests;

public class ContratosEnergiaPopupTests
{
    [Theory]
    [InlineData("ES0021000000000001AA", null, true, false)]
    [InlineData(null, "ES0234000000000002BB", false, true)]
    [InlineData("ES0021000000000001AA", "ES0234000000000002BB", true, true)]
    [InlineData(null, null, false, false)]
    [InlineData(" ", " ", false, false)]
    public void PopupMuestraCupsSegunSuministros(string? cupsLuz, string? cupsGas, bool muestraLuz, bool muestraGas)
    {
        var pagina = new ContratosEnergia();
        var auth = (AuthService)RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        SetField(auth, "_usuarioActual", new Usuario { Rol = "Administrador" });
        typeof(ContratosEnergia).GetProperty("AuthService", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(pagina, auth);
        SetField(pagina, "cargandoSesion", false);
        SetField(pagina, "contratoInfoFila", new Contrato
        {
            EnCups = cupsLuz,
            EnCupsGas = cupsGas,
            EnTarifa = "Tarifa luz de prueba",
            EnTarifaGas = "Tarifa gas de prueba"
        });

        var contenido = Renderizar(pagina);

        Assert.Equal(muestraLuz, contenido.Contains("CUPS Luz:"));
        Assert.Equal(muestraGas, contenido.Contains("CUPS Gas:"));
        if (muestraLuz) Assert.Contains(cupsLuz!, contenido);
        if (muestraGas) Assert.Contains(cupsGas!, contenido);
        Assert.Contains("Tarifa luz de prueba", contenido);
        Assert.Contains("Tarifa gas de prueba", contenido);
    }

    [Theory]
    [InlineData("Gas", null, "Tarifa gas", true)]
    [InlineData("Gas", "Tarifa luz", null, false)]
    [InlineData("Luz", "Tarifa luz", null, true)]
    [InlineData("Luz y Gas", "Tarifa luz", "Tarifa gas", true)]
    [InlineData("Luz y Gas", "Tarifa luz", null, false)]
    public void ValidarTarifaSegunTipoDeContratacion(
        string tipo,
        string? tarifaLuz,
        string? tarifaGas,
        bool valido)
    {
        var pagina = new ContratosEnergia();
        SetField(pagina, "contratoSeleccionado", new Contrato
        {
            EnTarifa = tarifaLuz,
            EnTarifaGas = tarifaGas
        });
        SetField(pagina, "tipoContratacion", tipo);

        var metodo = typeof(ContratosEnergia).GetMethod(
            "ValidarTarifaObligatoria",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var resultado = (bool)metodo.Invoke(pagina, null)!;

        Assert.Equal(valido, resultado);
        Assert.Equal(valido ? string.Empty : "La tarifa es obligatoria", GetField<string>(pagina, "mensajeError"));
    }

    [Fact]
    public void CerrarModalLimpiaElMensajeDeError()
    {
        var pagina = new ContratosEnergia();
        SetField(pagina, "mensajeError", "La tarifa es obligatoria");

        typeof(ContratosEnergia).GetMethod("CerrarModal", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(pagina, null);

        Assert.Equal(string.Empty, GetField<string>(pagina, "mensajeError"));
    }

    [Fact]
    public void FooterMuestraVersionActualizada()
    {
        var contenido = Renderizar(new Footer());
        Assert.Contains("Versión 20261103", contenido);
        Assert.DoesNotContain("Versión 20261021", contenido);
    }

    private static void SetField(object instancia, string nombre, object valor) =>
        instancia.GetType().GetField(nombre, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(instancia, valor);

    private static T GetField<T>(object instancia, string nombre) =>
        (T)instancia.GetType().GetField(nombre, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(instancia)!;

    // Inspección del renderizado sin inicializar servicios ni acceder a la base de datos.
#pragma warning disable BL0006
    private static string Renderizar(ComponentBase componente)
    {
        using var builder = new RenderTreeBuilder();
        componente.GetType().GetMethod("BuildRenderTree", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(componente, new object[] { builder });
        var frames = builder.GetFrames();
        var contenido = new StringBuilder();
        for (var i = 0; i < frames.Count; i++)
        {
            var frame = frames.Array[i];
            if (frame.FrameType == RenderTreeFrameType.Text)
                contenido.Append(frame.TextContent);
            else if (frame.FrameType == RenderTreeFrameType.Markup)
                contenido.Append(frame.MarkupContent);
        }
        return contenido.ToString();
    }
#pragma warning restore BL0006
}
