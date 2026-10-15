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

    [Fact]
    public void FooterMuestraVersionActualizada()
    {
        var contenido = Renderizar(new Footer());
        Assert.Contains("Versión 20261015", contenido);
        Assert.DoesNotContain("Versión 20261009", contenido);
    }

    private static void SetField(object instancia, string nombre, object valor) =>
        instancia.GetType().GetField(nombre, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(instancia, valor);

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
