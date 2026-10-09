using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EnerfoneCRM.Tests;

public class IncidenciasTests
{
    [Theory]
    [InlineData(6 * 1024 * 1024)]
    [InlineData(20 * 1024 * 1024)]
    public void AdmiteImagenesHastaVeinteMb(int tamano)
    {
        var imagen = ImagenIncidenciaAdjunta.Crear(CrearPng(tamano), "captura.png");
        Assert.Equal(tamano, imagen.Contenido.Length);
        Assert.Equal("image/png", imagen.TipoMime);
    }

    [Fact]
    public void RechazaImagenesMayoresDeVeinteMbOVacias()
    {
        Assert.Throws<ArgumentException>(() => ImagenIncidenciaAdjunta.Crear(CrearPng(20 * 1024 * 1024 + 1), "captura.png"));
        Assert.Throws<ArgumentException>(() => ImagenIncidenciaAdjunta.Crear(Array.Empty<byte>(), "captura.png"));
    }

    [Fact]
    public void UsaElFormatoRealYRechazaSvg()
    {
        var imagen = ImagenIncidenciaAdjunta.Crear(CrearPng(32), "captura.svg");
        Assert.Equal("captura.png", imagen.Nombre);
        Assert.Throws<ArgumentException>(() => ImagenIncidenciaAdjunta.Crear(Encoding.UTF8.GetBytes("<svg><script>alert(1)</script></svg>"), "captura.png"));
    }

    [Fact]
    public async Task GuardaLaImagenConUnNombreSeguro()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "incidencia-test-" + Guid.NewGuid().ToString("N"));
        var configuracion = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["StoragePath"] = carpeta }).Build();
        var servicio = new IncidenciaImagenService(configuracion);
        try
        {
            var imagen = ImagenIncidenciaAdjunta.Crear(CrearPng(32), "../captura.png");
            var nombre = await servicio.GuardarAsync(imagen);
            Assert.StartsWith("inc_", nombre);
            Assert.Equal(imagen.Contenido, await File.ReadAllBytesAsync(Path.Combine(carpeta, "incidencias", nombre)));
            servicio.EliminarSinGuardar(nombre);
            Assert.False(File.Exists(Path.Combine(carpeta, "incidencias", nombre)));
        }
        finally { if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true); }
    }

    [Fact]
    public void CambiaTipoPrioridadYEstadoSinAlterarLaDescripcion()
    {
        var incidencia = new Incidencia { Id = 117, Descripcion = "Descripcion original", UsuarioId = 9, Estado = "Pendiente", TipoIncidencia = "Error Técnico", Prioridad = "Alta" };
        var notificar = incidencia.AplicarCambiosGestion(new Incidencia { TipoIncidencia = "Solicitud de Mejora", Prioridad = "Media", Estado = "En validación" });
        Assert.Equal("Solicitud de Mejora", incidencia.TipoIncidencia);
        Assert.Equal("Media", incidencia.Prioridad);
        Assert.Equal("En validación", incidencia.Estado);
        Assert.Equal("Descripcion original", incidencia.Descripcion);
        Assert.Equal(9, incidencia.UsuarioId);
        Assert.NotNull(incidencia.FechaActualizacion);
        Assert.False(notificar);
    }

    [Fact]
    public void NotificaLaResolucionSoloCuandoCambiaAResuelta()
    {
        var incidencia = new Incidencia { Estado = "Pendiente" };
        var cambios = new Incidencia { TipoIncidencia = "Consulta", Prioridad = "Baja", Estado = "Resuelta" };
        Assert.True(incidencia.AplicarCambiosGestion(cambios));
        Assert.False(incidencia.AplicarCambiosGestion(cambios));
    }

    [Fact]
    public void RechazaClasificacionesInvalidasSinModificarElEstado()
    {
        var incidencia = new Incidencia { Estado = "Pendiente" };
        Assert.Throws<ArgumentException>(() => incidencia.AplicarCambiosGestion(new Incidencia { TipoIncidencia = "Consulta", Prioridad = "Inventada", Estado = "Resuelta" }));
        Assert.Equal("Pendiente", incidencia.Estado);
    }

    private static byte[] CrearPng(int tamano)
    {
        var contenido = new byte[tamano];
        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(contenido, 0);
        return contenido;
    }
}