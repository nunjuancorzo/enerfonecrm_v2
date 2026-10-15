using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EnerfoneCRM.Components.Pages;
using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace EnerfoneCRM.Tests;

public class EdicionMasivaTests
{
    [Theory]
    [InlineData("es-ES", "3.45", "3.45")]
    [InlineData("es-ES", "3,45", "3.45")]
    [InlineData("en-US", "3.45", "3.45")]
    [InlineData("en-US", "3,45", "3.45")]
    [InlineData("es-ES", "0.12345", "0.12345")]
    [InlineData("en-US", "0,12345", "0.12345")]
    [InlineData("es-ES", "1.234", "1.234")]
    [InlineData("en-US", "1,234", "1.234")]
    [InlineData("es-ES", " -12.50 ", "-12.50")]
    [InlineData("en-US", " +12,50 ", "12.50")]
    public void DecimalesNoDependenDeCultura(string cultura, string texto, string esperado)
    {
        var anterior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);
            Assert.True(DecimalPegado.TryParse(texto, out var resultado));
            Assert.Equal(decimal.Parse(esperado, CultureInfo.InvariantCulture), resultado);
        }
        finally
        {
            CultureInfo.CurrentCulture = anterior;
        }
    }

    [Theory]
    [InlineData("1.234,56")]
    [InlineData("1,234.56")]
    [InlineData("1.2.3")]
    [InlineData("no")]
    public void DecimalesInvalidosProducenErrorVisibleEnLaImportacion(string texto)
    {
        Assert.Throws<FormatException>(() => DecimalPegado.Validar(new[] { texto }, 0));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void DecimalVacioMantieneComportamientoOpcional(string? texto)
    {
        Assert.False(DecimalPegado.TryParse(texto, out _));
        DecimalPegado.Validar(new[] { texto ?? "" }, 0);
    }

    [Theory]
    [InlineData("energia", 23)]
    [InlineData("telefonia", 99)]
    [InlineData("alarma", 13)]
    public void ColumnasAdicionalesSonPersistentesYSeCopianSinPerdidas(string tipo, int cantidad)
    {
        var campos = CamposContratoEdicionMasiva.Obtener(tipo);
        Assert.Equal(cantidad, campos.Count);
        Assert.Equal(campos.Count, campos.Select(c => c.Nombre).Distinct().Count());
        var original = new Contrato();
        foreach (var campo in campos)
        {
            var propiedad = typeof(Contrato).GetProperty(campo.Nombre)!;
            Assert.NotNull(propiedad.GetCustomAttribute<ColumnAttribute>());
            Assert.Null(propiedad.GetCustomAttribute<NotMappedAttribute>());
            campo.Asignar(original, Ejemplo(campo));
        }
        var columnas = Enumerable.Repeat("", CamposContratoEdicionMasiva.ColumnasOriginales(tipo))
            .Concat(CamposContratoEdicionMasiva.Valores(original, tipo).Split('\t')).ToArray();
        var destino = new Contrato();
        CamposContratoEdicionMasiva.Aplicar(destino, tipo, columnas);
        Assert.Equal(CamposContratoEdicionMasiva.Valores(original, tipo),
            CamposContratoEdicionMasiva.Valores(destino, tipo));
    }

    [Fact]
    public void TelefoniaIncluyeTodosLosCamposDeLasQuinceLineas()
    {
        var nombres = CamposContratoEdicionMasiva.Obtener("telefonia").Select(c => c.Nombre).ToHashSet();
        for (var i = 1; i <= 15; i++)
            foreach (var nombre in new[] { $"TelefonoLinea{i}Tel", $"TarifaLinea{i}Tel", $"TipoLinea{i}Tel",
                         $"CodigoIccLinea{i}Tel", $"AltaNuevaLinea{i}Tel" })
                Assert.Contains(nombre, nombres);
    }

    [Theory]
    [InlineData("energia")]
    [InlineData("telefonia")]
    [InlineData("alarma")]
    public void HojaAntiguaNoBorraNuevosCampos(string tipo)
    {
        var contrato = new Contrato { IdContratoExterno = "EXTERNO", TitularIbanNombre = "Titular", CargadoEnPlataforma = true };
        var columnas = Enumerable.Repeat("", CamposContratoEdicionMasiva.ColumnasOriginales(tipo)).ToArray();
        CamposContratoEdicionMasiva.Aplicar(contrato, tipo, columnas);
        CamposContratoEdicionMasiva.Copiar(new Contrato(), contrato, tipo, columnas.Length);
        Assert.Equal("EXTERNO", contrato.IdContratoExterno);
        Assert.Equal("Titular", contrato.TitularIbanNombre);
        Assert.True(contrato.CargadoEnPlataforma);
    }

    [Fact]
    public void FechaIncorrectaNoModificaParcialmenteLaExtension()
    {
        var contrato = new Contrato { FechaAlta = new DateTime(2026, 10, 15), IdContratoExterno = "Anterior" };
        var columnas = Enumerable.Repeat("", 27).Concat(new[] { "31/02/2026", "Nuevo" }).ToArray();
        Assert.Throws<FormatException>(() => CamposContratoEdicionMasiva.Aplicar(contrato, "energia", columnas));
        Assert.Equal("Anterior", contrato.IdContratoExterno);
        Assert.Equal(new DateTime(2026, 10, 15), contrato.FechaAlta);
    }

    [Theory]
    [InlineData("energia", false)]
    [InlineData("energia", true)]
    [InlineData("telefonia", false)]
    [InlineData("telefonia", true)]
    [InlineData("alarma", false)]
    [InlineData("alarma", true)]
    public async Task PegadoRealIncluyeNuevosCamposYDecimal(string tipo, bool desdeTexto)
    {
        var pagina = CrearPagina(tipo);
        using var renderer = new RendererPrueba();
        renderer.Adjuntar(pagina);
        var columnas = FilaEjemplo(tipo);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var metodo = pagina.GetType().GetMethod(desdeTexto ? "PegarDesdePortapapeles" : "PasteRowsFromJS")!;
            object entrada = desdeTexto ? string.Join("\t", columnas) : new List<List<string>> { columnas };
            await (Task)metodo.Invoke(pagina, new[] { entrada })!;
        });
        Assert.Empty(renderer.Errores);
        var contratos = GetField<List<Contrato>>(pagina, "contratos");
        var contrato = Assert.Single(contratos);
        Assert.Equal(3.45m, contrato.Comision);
        if (tipo == "energia") Assert.Equal(0.12345m, contrato.PotenciaContratadaP1);
        foreach (var campo in CamposContratoEdicionMasiva.Obtener(tipo))
            Assert.Equal(campo.Leer(ContratoEjemplo(tipo)), campo.Leer(contrato));
    }

    [Theory]
    [InlineData("energia")]
    [InlineData("telefonia")]
    [InlineData("alarma")]
    public async Task CopiaRealIncluyeLasCabecerasYLosValores(string tipo)
    {
        var pagina = CrearPagina(tipo);
        using var renderer = new RendererPrueba();
        renderer.Adjuntar(pagina);
        var js = new PortapapelesPrueba();
        pagina.GetType().GetProperty("JSRuntime", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(pagina, js);
        var contrato = ContratoEjemplo(tipo);
        GetField<List<Contrato>>(pagina, "contratos").Add(contrato);
        GetField<HashSet<int>>(pagina, "filasSeleccionadas").Add(0);
        await renderer.Dispatcher.InvokeAsync(async () =>
            await (Task)pagina.GetType().GetMethod("CopiarFilasSeleccionadas", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(pagina, null)!);
        var filas = js.Texto!.Split('\n');
        Assert.EndsWith(CamposContratoEdicionMasiva.Cabeceras(tipo), filas[0]);
        Assert.EndsWith(CamposContratoEdicionMasiva.Valores(contrato, tipo), filas[1]);
        Assert.Equal(filas[0].Split('\t').Length, filas[1].Split('\t').Length);
    }

    [Theory]
    [InlineData("energia", false)]
    [InlineData("energia", true)]
    [InlineData("telefonia", false)]
    [InlineData("telefonia", true)]
    [InlineData("alarma", false)]
    [InlineData("alarma", true)]
    public async Task ActualizarFilaConservaCamposAusentesYAplicaLosPresentes(string tipo, bool desdeTexto)
    {
        var pagina = CrearPagina(tipo);
        using var renderer = new RendererPrueba();
        renderer.Adjuntar(pagina);
        var contrato = ContratoEjemplo(tipo);
        contrato.Id = 41;
        GetField<List<Contrato>>(pagina, "contratos").Add(contrato);
        var fila = FilaEjemplo(tipo);
        fila[0] = "41";
        await Pegar(fila.Take(CamposContratoEdicionMasiva.ColumnasOriginales(tipo)).ToList());
        Assert.Same(contrato, Assert.Single(GetField<List<Contrato>>(pagina, "contratos")));
        Assert.Equal("Prueba", contrato.IdContratoExterno);
        fila[CamposContratoEdicionMasiva.ColumnasOriginales(tipo) + 1] = "NUEVO-EXTERNO";
        await Pegar(fila);
        Assert.Equal("NUEVO-EXTERNO", contrato.IdContratoExterno);
        Assert.True(contrato.CargadoEnPlataforma);
        Assert.Empty(renderer.Errores);

        Task Pegar(List<string> columnas) => renderer.Dispatcher.InvokeAsync(async () =>
        {
            var metodo = pagina.GetType().GetMethod(desdeTexto ? "PegarDesdePortapapeles" : "PasteRowsFromJS")!;
            object entrada = desdeTexto ? string.Join("\t", columnas) : new List<List<string>> { columnas };
            await (Task)metodo.Invoke(pagina, new[] { entrada })!;
        });
    }

    [Theory]
    [InlineData("energia", 52)]
    [InlineData("telefonia", 123)]
    [InlineData("alarma", 40)]
    public async Task TablaRenderizaUnaCeldaPorCadaCabecera(string tipo, int columnas)
    {
        var pagina = CrearPagina(tipo);
        var auth = (AuthService)RuntimeHelpers.GetUninitializedObject(typeof(AuthService));
        typeof(AuthService).GetField("_usuarioActual", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(auth, new Usuario { Rol = "Administrador" });
        pagina.GetType().GetProperty("AuthService", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(pagina, auth);
        pagina.GetType().GetField("inicializando", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(pagina, false);
        GetField<List<Contrato>>(pagina, "contratos").Add(ContratoEjemplo(tipo));
        using var renderer = new RendererPrueba();
        var id = renderer.Adjuntar(pagina);
        await renderer.Dispatcher.InvokeAsync(() =>
            typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(pagina, null));
        Assert.Empty(renderer.Errores);
        Assert.Equal(columnas, renderer.ContarElementos(id, "th"));
        Assert.Equal(columnas, renderer.ContarElementos(id, "td"));
    }

    private static ComponentBase CrearPagina(string tipo)
    {
        ComponentBase pagina = tipo switch
        {
            "energia" => new ContratosEnergiaEdicionMasiva(),
            "telefonia" => new ContratosTelefoniaEdicionMasiva(),
            _ => new ContratosAlarmasEdicionMasiva()
        };
        pagina.GetType().GetProperty("JSRuntime", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(pagina, new PortapapelesPrueba());
        return pagina;
    }

    private static T GetField<T>(object objeto, string nombre) =>
        (T)objeto.GetType().GetField(nombre, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(objeto)!;

    private static string Ejemplo(CampoContratoEdicionMasiva campo) => campo.TipoEntrada switch
    {
        "date" => "2026-10-15",
        "number" => "123",
        "boolean" => "1",
        _ when campo.Nombre.StartsWith("CodigoIcc") => "8934000000000000001",
        _ => "Prueba"
    };

    private static Contrato ContratoEjemplo(string tipo)
    {
        var contrato = new Contrato();
        foreach (var campo in CamposContratoEdicionMasiva.Obtener(tipo))
            campo.Asignar(contrato, Ejemplo(campo));
        return contrato;
    }

    private static List<string> FilaEjemplo(string tipo)
    {
        var columnas = Enumerable.Repeat("", CamposContratoEdicionMasiva.ColumnasOriginales(tipo)).ToList();
        columnas[2] = "Cliente de prueba";
        columnas[tipo == "energia" ? 25 : tipo == "telefonia" ? 20 : 22] = "3.45";
        if (tipo == "energia") columnas[11] = "0,12345";
        columnas.AddRange(CamposContratoEdicionMasiva.Valores(ContratoEjemplo(tipo), tipo).Split('\t'));
        return columnas;
    }

    private sealed class PortapapelesPrueba : IJSRuntime
    {
        public string? Texto { get; private set; }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (identifier == "navigator.clipboard.writeText") Texto = (string)args![0]!;
            return ValueTask.FromResult(default(TValue)!);
        }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, System.Threading.CancellationToken token, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
    }

    // Adjunta las páginas sin ejecutar su carga de base de datos.
#pragma warning disable BL0006
    private sealed class RendererPrueba : Renderer
    {
        public RendererPrueba() : base(new ServiceCollection()
            .AddSingleton(new CodigoPostalService(new DbContextProvider(
                new ConfigurationBuilder().Build(), new HttpContextAccessor())))
            .BuildServiceProvider(), NullLoggerFactory.Instance) { }
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        public List<Exception> Errores { get; } = new();
        public int Adjuntar(IComponent component) => AssignRootComponentId(component);
        public int ContarElementos(int id, string elemento)
        {
            var frames = GetCurrentRenderTreeFrames(id);
            var cantidad = 0;
            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames.Array[i];
                if (frame.FrameType == RenderTreeFrameType.Element && frame.ElementName == elemento)
                    cantidad++;
                else if (frame.FrameType == RenderTreeFrameType.Markup)
                    cantidad += System.Text.RegularExpressions.Regex.Matches(frame.MarkupContent, $"<{elemento}(?:\\s|>)").Count;
                else if (frame.FrameType == RenderTreeFrameType.Component)
                    cantidad += ContarElementos(frame.ComponentId, elemento);
            }
            return cantidad;
        }
        protected override void HandleException(Exception exception) => Errores.Add(exception);
        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }
#pragma warning restore BL0006
}
