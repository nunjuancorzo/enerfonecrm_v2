using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnerfoneCRM.Services;
using EnerfoneCRM.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.IO;
using Microsoft.AspNetCore.DataProtection;

namespace EnerfoneCRM.Tests;

public class GeminiServiceTests
{
    [Fact]
    public async Task AutorizacionVoltiPersistePorEmpresaEntreUsuariosYSePuedeRevocar()
    {
        var directorio = Path.Combine(Path.GetTempPath(), "volti-consent-" + Guid.NewGuid().ToString("N"));
        var configuracion = new ConfigurationBuilder().AddInMemoryCollection(new[]
        {
            new System.Collections.Generic.KeyValuePair<string, string?>("Gemini:SecretsDirectory", directorio)
        }).Build();
        var proteccion = new EphemeralDataProtectionProvider();
        try
        {
            var administrador = new ConsentimientoVoltiService(configuracion, proteccion, () => (1, 1, true));
            Assert.False(await administrador.EstaAutorizadaAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => administrador.ExigirAutorizacionAsync());
            await administrador.AutorizarAsync();
            var otroUsuario = new ConsentimientoVoltiService(configuracion, proteccion, () => (1, 2, false));
            Assert.True(await otroUsuario.EstaAutorizadaAsync());
            await otroUsuario.ExigirAutorizacionAsync();
            var otraEmpresa = new ConsentimientoVoltiService(configuracion, proteccion, () => (2, 3, true));
            Assert.False(await otraEmpresa.EstaAutorizadaAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => otraEmpresa.ExigirAutorizacionAsync());
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => otroUsuario.AutorizarAsync());
            await administrador.RevocarAsync();
            Assert.False(await otroUsuario.EstaAutorizadaAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => otroUsuario.ExigirAutorizacionAsync());
        }
        finally { if (Directory.Exists(directorio)) Directory.Delete(directorio, recursive: true); }
    }

    [Fact]
    public void LeeTextoEstructurado()
    {
        using var respuesta = JsonDocument.Parse("{\"status\":\"completed\",\"output_text\":\"respuesta\"}");
        Assert.Equal("respuesta", GeminiService.LeerRespuesta(respuesta.RootElement));
    }

    [Fact]
    public void LeeGenerateContentYRechazaResultadosIncompletos()
    {
        using var respuesta = JsonDocument.Parse("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"thought\":true,\"text\":\"interno\"},{\"text\":\"respuesta\"}]}}]}");
        Assert.Equal("respuesta", GeminiService.LeerRespuesta(respuesta.RootElement));
        using var incompleta = JsonDocument.Parse("{\"candidates\":[{\"finishReason\":\"MAX_TOKENS\"}]}");
        Assert.Throws<InvalidOperationException>(() => GeminiService.LeerRespuesta(incompleta.RootElement));
    }

    [Fact]
    public void ErrorGeminiDistingueMetodoNoDisponibleSinMostrarMensajeNiClave()
    {
        var mensaje = GeminiService.DescribirError(HttpStatusCode.NotImplemented,
            "{\"error\":{\"status\":\"UNIMPLEMENTED\",\"message\":\"clave-super-secreta\"}}");
        Assert.Contains("HTTP 501", mensaje);
        Assert.Contains("UNIMPLEMENTED", mensaje);
        Assert.DoesNotContain("clave-super-secreta", mensaje);
    }

    [Fact]
    public void ErrorGemini503IndicaSaturacionTemporalYNoExponeRespuestaRemota()
    {
        var mensaje = GeminiService.DescribirError(HttpStatusCode.ServiceUnavailable,
            "{\"error\":{\"status\":\"UNAVAILABLE\",\"message\":\"contenido-remoto\"}}");
        Assert.Contains("temporalmente saturado", mensaje);
        Assert.Contains("HTTP 503", mensaje);
        Assert.Contains("UNAVAILABLE", mensaje);
        Assert.DoesNotContain("contenido-remoto", mensaje);
    }

    [Fact]
    public void ErrorGeminiExplicaRestriccionesDeClave()
    {
        var mensaje = GeminiService.DescribirError(HttpStatusCode.Forbidden,
            "{\"error\":{\"status\":\"PERMISSION_DENIED\",\"details\":[{\"reason\":\"API_KEY_HTTP_REFERRER_BLOCKED\"}]}}");
        Assert.Contains("restricciones", mensaje);
        Assert.Contains("HTTP 403", mensaje);
    }

    [Fact]
    public void LeeLosPasosRestSinIncluirPensamientos()
    {
        using var respuesta = JsonDocument.Parse("{\"status\":\"completed\",\"steps\":[{\"type\":\"thought\",\"summary\":[{\"type\":\"text\",\"text\":\"interno\"}]},{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"respuesta\"}]}]}");
        Assert.Equal("respuesta", GeminiService.LeerRespuesta(respuesta.RootElement));
    }

    [Theory]
    [InlineData("DNI 12345678Z", "12345678Z")]
    [InlineData("Cuenta ES91 2100 0418 4502 0005 1332", "2100")]
    [InlineData("Contacto ejemplo@example.invalid", "example.invalid")]
    [InlineData("Movil +34 612 345 678", "612")]
    public void VoltiEnmascaraIdentificadoresEnTextosLibres(string texto, string identificador)
    {
        Assert.DoesNotContain(identificador, VoltiService.LimpiarContexto(texto));
        Assert.Contains("[dato protegido]", VoltiService.LimpiarContexto(texto));
    }

    [Fact]
    public void FacturaGeminiConservaRevisionYNoAdmiteCamposArbitrarios()
    {
        var factura = GeminiFacturaService.Interpretar("{\"consumo_p1\":\"125.5\",\"tipo_suministro\":\"Luz\"}");
        Assert.Equal("125.5", factura.Obtener("consumo_p1"));
        Assert.Contains("revision", factura.Campos.Find(campo => campo.Clave == "consumo_p1")!.Evidencia);
        Assert.Throws<InvalidOperationException>(() => GeminiFacturaService.Interpretar("{\"empresa_id\":\"2\"}"));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"tipo_suministro\":\"Luz\",\"consumo_total\":\"125\"}")]
    [InlineData("{\"fecha_inicio\":\"2021-05-31\",\"fecha_fin\":\"2021-06-22\",\"total_factura\":\"32.74\",\"consumo_total\":\"125\"}")]
    public void FacturaGeminiNoMuestraAvisosDeDatosNoDetectados(string json)
    {
        Assert.Empty(GeminiFacturaService.Interpretar(json).Advertencias);
    }

    [Fact]
    public void FacturaGeminiConservaAvisosDeDatosIncoherentes()
    {
        var factura = GeminiFacturaService.Interpretar("{\"fecha_inicio\":\"2021-06-22\",\"fecha_fin\":\"2021-05-31\",\"total_factura\":\"32.74\"}");
        Assert.Contains(factura.Advertencias, aviso => aviso.Contains("invertidas"));
        Assert.NotEmpty(FacturaImportacionService.ValidarParaAplicar(factura));
    }

    [Fact]
    public void VoltiNoAdmiteSqlNiConsultasSinLimite()
    {
        var consulta = new ConsultaVolti("contratos", "listado", "", "", "", "", "", 10, "recientes");
        Assert.Equal(consulta, VoltiService.Interpretar(JsonSerializer.Serialize(consulta)));
        Assert.Throws<InvalidOperationException>(() => VoltiService.Interpretar(JsonSerializer.Serialize(consulta with { Modulo = "usuarios" })));
        Assert.Throws<InvalidOperationException>(() => VoltiService.Interpretar(JsonSerializer.Serialize(consulta with { Limite = 1000 })));
        Assert.Throws<JsonException>(() => VoltiService.Interpretar(JsonSerializer.Serialize(consulta)[..^1] + ",\"Sql\":\"DELETE FROM contratos\"}"));
    }

    [Fact]
    public void VoltiFiltraFechasEstadoYTipoAntesDeResumir()
    {
        var consulta = new ConsultaVolti("contratos", "resumen", "Activo", "energia", "", "2026-01-01", "2026-01-31", 10, "recientes");
        var registros = new[]
        {
            new RegistroVolti(1, "Cliente A", "Act/Facturable", "energia", new DateTime(2026, 1, 31, 23, 0, 0)),
            new RegistroVolti(2, "Cliente B", "Activo", "energia", new DateTime(2026, 2, 1)),
            new RegistroVolti(3, "Cliente C", "Activo", "telefonia", new DateTime(2026, 1, 10))
        };
        Assert.Equal(1, Assert.Single(VoltiService.Filtrar(registros, consulta)).Id);
    }

    [Theory]
    [InlineData("Activo")]
    [InlineData("activos")]
    [InlineData("Facturable")]
    public void VoltiReconoceVariantesDelEstadoActivo(string estado)
    {
        var consulta = new ConsultaVolti("contratos", "listado", estado, "Energía", "", "", "", 10, "recientes");
        var registros = new[]
        {
            new RegistroVolti(1, "Cliente activo", "Act/Facturable", "energia", DateTime.Today),
            new RegistroVolti(2, "Contrato activo", "Activo", "energía", DateTime.Today),
            new RegistroVolti(3, "Cliente inactivo", "Baja", "energia", DateTime.Today),
            new RegistroVolti(4, "Contrato telefonía", "Activo", "telefonia", DateTime.Today)
        };

        Assert.Equal(new[] { 2, 1 }, VoltiService.Filtrar(registros, consulta).Select(registro => registro.Id));
    }

    [Fact]
    public void VoltiIncluyeMuestrasTambienEnConsultasDeResumen()
    {
        var consulta = new ConsultaVolti("clientes", "resumen", "", "", "", "", "", 2, "recientes");
        var registros = new[]
        {
            new RegistroVolti(1, "Cliente uno", "", "Particular", DateTime.Today),
            new RegistroVolti(2, "Cliente dos", "", "Pyme", DateTime.Today.AddDays(-1)),
            new RegistroVolti(3, "Cliente tres", "", "Particular", DateTime.Today.AddDays(-2))
        };

        Assert.Equal(new[] { 1, 2 }, VoltiService.SeleccionarMuestras(registros, consulta).Select(registro => registro.Id));
    }

    [Fact]
    public async Task SinClaveNoHacePeticiones()
    {
        var servicio = new GeminiService(new HttpClient(), new ConfigurationBuilder().Build());
        Assert.False(servicio.Disponible);
        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.GenerarAsync("instrucciones", "pregunta"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.ObtenerModelosAsync());
    }

    [Fact]
    public async Task EnviaClaveEnCabeceraSinIncluirSecretosNiEstadoEnLaSolicitud()
    {
        var handler = new RespuestaGemini();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new[]
        {
            new System.Collections.Generic.KeyValuePair<string, string?>("Gemini:ApiKey", "clave-ficticia")
        }).Build();
        var servicio = new GeminiService(new HttpClient(handler), configuration);
        Assert.Equal("respuesta", await servicio.GenerarAsync("instrucciones", "pregunta"));
        Assert.Equal("clave-ficticia", handler.Clave);
        using var payload = JsonDocument.Parse(handler.Payload!);
        Assert.True(payload.RootElement.TryGetProperty("contents", out _));
        Assert.False(payload.RootElement.TryGetProperty("previous_interaction_id", out _));
        Assert.EndsWith(":generateContent", handler.Ruta);
        Assert.DoesNotContain("clave-ficticia", handler.Payload);
    }

    private class RespuestaGemini : HttpMessageHandler
    {
        public string? Clave;
        public string? Payload;
        public string? Ruta;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Clave = string.Join("", request.Headers.GetValues("x-goog-api-key"));
            Ruta = request.RequestUri!.AbsolutePath;
            Payload = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"text\":\"respuesta\"}]}}]}", Encoding.UTF8, "application/json")
            };
        }
    }
}