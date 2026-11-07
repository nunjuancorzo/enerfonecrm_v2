using System;
using System.Linq;
using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EnerfoneCRM.Tests;

public class EmpresaIsolationTests
{
    private static Cliente CrearCliente(int id) => new() { Id = id, Nombre = "Cliente prueba", TipoCliente = "Particular" };
    private static ApplicationDbContext CrearContexto(int empresaId, bool superadmin = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql("Server=localhost;Database=prueba;User=test", new MySqlServerVersion(new Version(8, 0, 36))).Options;
        return new ApplicationDbContext(options, empresaId, superadmin);
    }

    [Fact]
    public void TodasLasEntidadesTienenEmpresaYFiltro()
    {
        using var context = CrearContexto(1);
        foreach (var entity in context.Model.GetEntityTypes().Where(entity => entity.ClrType != typeof(Empresa)))
        {
            Assert.Equal("id_empresa", entity.FindProperty("EmpresaId")!.GetColumnName());
            Assert.NotNull(entity.GetQueryFilter());
            Assert.True(entity.FindProperty("EmpresaId")!.IsConcurrencyToken);
        }
    }

    [Fact]
    public void LosFiltrosUsanLaEmpresaDeCadaContexto()
    {
        using var primera = CrearContexto(1);
        using var segunda = CrearContexto(2);
        Assert.Contains("id_empresa", primera.Clientes.ToQueryString());
        Assert.Contains("= 1", primera.Clientes.ToQueryString());
        Assert.Contains("= 2", segunda.Clientes.ToQueryString());
    }

    [Fact]
    public void NoPermiteGuardarEnOtraEmpresa()
    {
        using var context = CrearContexto(1, true);
        context.Usuarios.Add(new Usuario { EmpresaId = 2 });
        Assert.Throws<UnauthorizedAccessException>(() => context.SaveChanges());
    }

    [Fact]
    public void AdministradorNoPuedeCrearUsuariosNiEmpresas()
    {
        using var usuarios = CrearContexto(1);
        usuarios.Usuarios.Add(new Usuario());
        Assert.Throws<UnauthorizedAccessException>(() => usuarios.SaveChanges());
        using var empresas = CrearContexto(1);
        empresas.Empresas.Add(new Empresa { Nombre = "Otra" });
        Assert.Throws<UnauthorizedAccessException>(() => empresas.SaveChanges());
        using var configuracion = CrearContexto(1);
        configuracion.ConfiguracionesEmpresa.Add(new ConfiguracionEmpresa { NombreEmpresa = "Empresa" });
        Assert.Throws<UnauthorizedAccessException>(() => configuracion.SaveChanges());
    }

    [Fact]
    public void LecturasYActualizacionesNoCruzanEmpresas()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var primera = new ApplicationDbContext(options, 1))
        {
            primera.Clientes.Add(CrearCliente(1));
            primera.SaveChanges();
        }
        using (var segunda = new ApplicationDbContext(options, 2))
        {
            segunda.Clientes.Add(CrearCliente(2));
            segunda.SaveChanges();
        }
        using var context = new ApplicationDbContext(options, 1);
        Assert.Equal(1, Assert.Single(context.Clientes).Id);
        Assert.Null(context.Clientes.Find(2));
        context.Clientes.Update(CrearCliente(2));
        Assert.Throws<UnauthorizedAccessException>(() => context.SaveChanges());
    }

    [Fact]
    public void NoSePuedeRelacionarContratoConClienteDeOtraEmpresa()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var segunda = new ApplicationDbContext(options, 2))
        {
            segunda.Clientes.Add(CrearCliente(1));
            segunda.SaveChanges();
        }
        using var primera = new ApplicationDbContext(options, 1);
        primera.Contratos.Add(new Contrato { IdCliente = 1 });
        Assert.Throws<UnauthorizedAccessException>(() => primera.SaveChanges());
    }

    [Fact]
    public void NuevosRegistrosRecibenLaEmpresaYSePuedenActualizarDesconectados()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var primera = new ApplicationDbContext(options, 2))
        {
            var cliente = CrearCliente(1);
            primera.Clientes.Add(cliente);
            primera.SaveChanges();
            Assert.Equal(2, primera.Entry(cliente).Property<int>("EmpresaId").CurrentValue);
        }
        using var context = new ApplicationDbContext(options, 2);
        context.Clientes.Update(CrearCliente(1));
        Assert.Equal(1, context.SaveChanges());
    }

    [Fact]
    public void SinEmpresaNoHayDatosNiEscriturasInclusoParaSuperadmin()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var primera = new ApplicationDbContext(options, 1))
        {
            primera.Clientes.Add(CrearCliente(1));
            primera.SaveChanges();
        }
        using var sinEmpresa = new ApplicationDbContext(options, 0, true);
        Assert.Empty(sinEmpresa.Clientes);
        sinEmpresa.Clientes.Add(new Cliente());
        Assert.Throws<UnauthorizedAccessException>(() => sinEmpresa.SaveChanges());
    }
}