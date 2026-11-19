using System;
using System.Linq;
using System.Threading.Tasks;
using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using EnerfoneCRM.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
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
        foreach (var entity in context.Model.GetEntityTypes().Where(entity => entity.ClrType != typeof(Empresa) && entity.ClrType != typeof(PerfilSuperAdmin)))
        {
            Assert.Equal("id_empresa", entity.FindProperty("EmpresaId")!.GetColumnName());
            Assert.True(entity.GetQueryFilter() != null, $"Falta el filtro de empresa en {entity.ClrType.Name}.");
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
    public void PerfilPropioSoloPermiteDatosPersonalesConFlag()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var admin = new ApplicationDbContext(options, 1, esSuperAdmin: true))
        {
            admin.Usuarios.Add(new Usuario { Id = 5, NombreUsuario = "comercial", Nombre = "Ana", Activo = true });
            admin.SaveChanges();
        }

        using (var sinFlag = new ApplicationDbContext(options, 1))
        {
            sinFlag.Usuarios.Find(5)!.Nombre = "Otra";
            Assert.Throws<UnauthorizedAccessException>(() => sinFlag.SaveChanges());
        }

        using var propio = new ApplicationDbContext(options, 1) { PermitirEdicionPerfilPropio = true };
        var usuario = propio.Usuarios.Find(5)!;
        usuario.Nombre = "Ana María";
        propio.SaveChanges();

        usuario.Activo = false;
        Assert.Throws<UnauthorizedAccessException>(() => propio.SaveChanges());
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task FirmaPublicaUsaLaEmpresaDelTokenSinCambiarLaSesion(int empresaSesion)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        using (var admin = new ApplicationDbContext(options, 1, esSuperAdmin: true))
        {
            admin.Empresas.AddRange(new Empresa { Id = 1, Nombre = "Empresa A" }, new Empresa { Id = 2, Nombre = "Empresa B" });
            admin.SaveChanges();
        }

        var token = new FirmaTokenService().Generar();
        using (var segunda = new ApplicationDbContext(options, 2))
        {
            segunda.Contratos.Add(new Contrato { Id = 20, NombreCliente = "Cliente B", Estado = "Pte Firma" });
            segunda.FicherosContratos.AddRange(
                new FicheroContrato { Id = 21, IdContrato = 20, Fichero = new byte[] { 1 } },
                new FicheroContrato { Id = 22, IdContrato = 20, Fichero = new byte[] { 2 } });
            segunda.SolicitudesFirma.Add(new SolicitudFirma
            {
                Id = 23,
                ContratoId = 20,
                DocumentoOriginalId = 21,
                TokenHash = new FirmaTokenService().Hash(token),
                FechaCaducidadUtc = DateTime.UtcNow.AddDays(1),
                Estado = EstadoSolicitudFirma.Enviado
            });
            segunda.SaveChanges();
        }

        var sesion = new EmpresaContext();
        sesion.Establecer(empresaSesion);
        var configuration = new ConfigurationBuilder().Build();
        var http = new HttpContextAccessor();
        var provider = new DbContextProviderEnMemoria(options, configuration, http, sesion);
        var pdf = new ContractSigningPdfService(provider);
        var servicio = new FirmaService(provider, new EmailService(provider), pdf, configuration, http, NullLogger<FirmaService>.Instance);

        Assert.Null(await servicio.ObtenerSolicitudPublicaAsync("token-no-valido"));
        var solicitud = await servicio.ObtenerSolicitudPublicaAsync(token);
        Assert.NotNull(solicitud);
        Assert.Equal(EstadoSolicitudFirma.EnProceso, solicitud.Estado);
        Assert.Equal(new byte[] { 1 }, (await servicio.ObtenerDocumentoPublicoAsync(token, 21)).Contenido);
        Assert.Null((await servicio.ObtenerDocumentoPublicoAsync(token, 22)).Contenido);

        var firma = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jR5sAAAAASUVORK5CYII=");
        var resultado = await servicio.CompletarFirmaAsync(token, true, firma);
        Assert.True(resultado.Exito, resultado.Mensaje);
        Assert.Equal(empresaSesion, sesion.EmpresaId);

        using var otraEmpresa = new ApplicationDbContext(options, 1);
        using var empresaToken = new ApplicationDbContext(options, 2);
        Assert.Empty(otraEmpresa.SolicitudesFirmaEventos);
        Assert.Equal(new[] { "EnlaceAbierto", "DocumentoConsultado", "FirmaCompletada" },
            empresaToken.SolicitudesFirmaEventos.OrderBy(e => e.Id).Select(e => e.TipoEvento).ToArray());
        var firmada = Assert.Single(empresaToken.SolicitudesFirma);
        Assert.Equal(EstadoSolicitudFirma.Firmado, firmada.Estado);
        Assert.Equal("Firmado", Assert.Single(empresaToken.Contratos).Estado);
        var documentoFirmado = await servicio.ObtenerDocumentoPublicoAsync(token, firmada.DocumentoFirmadoId!.Value);
        Assert.NotNull(documentoFirmado.Contenido);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(documentoFirmado.Contenido, 0, 5));
    }

    [Fact]
    public async Task FirmaManualConservaElPdfAparteDeLaFirmaPorCorreoYRespetaLaEmpresa()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var context = new ApplicationDbContext(options, 2))
        {
            context.Contratos.AddRange(new Contrato { Id = 20, Estado = "Pte Firma" }, new Contrato { Id = 21 });
            context.FicherosContratos.Add(new FicheroContrato { Id = 22, IdContrato = 20, TipoFichero = "DocumentoFirmaOriginal", Fichero = new byte[] { 1 } });
            context.SolicitudesFirma.Add(new SolicitudFirma { ContratoId = 20, DocumentoOriginalId = 22, Estado = EstadoSolicitudFirma.Enviado });
            context.SaveChanges();
        }

        var sesion = new EmpresaContext();
        sesion.Establecer(2);
        var configuration = new ConfigurationBuilder().Build();
        var http = new HttpContextAccessor();
        var provider = new DbContextProviderEnMemoria(options, configuration, http, sesion);
        var pdf = new ContractSigningPdfService(provider);
        var servicio = new FirmaService(provider, new EmailService(provider), pdf, configuration, http, NullLogger<FirmaService>.Instance);

        var original = await servicio.GenerarDocumentoFirmaManualAsync(20);
        Assert.NotNull(original);
        Assert.True(original.AsSpan().StartsWith("%PDF-"u8));
        Assert.Empty(await servicio.ObtenerDocumentosFirmaManualAsync(20));
        Assert.False((await servicio.GuardarDocumentoFirmaManualAsync(20, "falso.pdf", new byte[] { 1 })).Exito);
        Assert.False((await servicio.GuardarDocumentoFirmaManualAsync(20, "grande.pdf", new byte[10 * 1024 * 1024 + 1])).Exito);
        Assert.True((await servicio.GuardarDocumentoFirmaManualAsync(20, "firmado.pdf", original)).Exito);

        var firmado = Assert.Single(await servicio.ObtenerDocumentosFirmaManualAsync(20));
        Assert.NotNull(firmado.Nombre);
        Assert.EndsWith(".pdf", firmado.Nombre);
        Assert.Equal(original, (await servicio.ObtenerDocumentoFirmaManualAsync(20, firmado.Id))?.Fichero);
        Assert.Null(await servicio.ObtenerDocumentoFirmaManualAsync(20, 22));
        Assert.Null(await servicio.ObtenerDocumentoFirmaManualAsync(21, firmado.Id));
        Assert.Null((await servicio.ObtenerDocumentoPrivadoAsync(20, firmado.Id)).Contenido);
        Assert.Equal(EstadoSolicitudFirma.Enviado, (await servicio.ObtenerUltimaSolicitudAsync(20))?.Estado);

        using var empresa = new ApplicationDbContext(options, 2);
        Assert.Equal("Pte Firma", empresa.Contratos.Single(c => c.Id == 20).Estado);
        sesion.Establecer(1);
        Assert.Empty(await servicio.ObtenerDocumentosFirmaManualAsync(20));
        Assert.Null(await servicio.GenerarDocumentoFirmaManualAsync(20));
        Assert.False((await servicio.GuardarDocumentoFirmaManualAsync(20, "firmado.pdf", original)).Exito);
    }

    [Theory]
    [InlineData(null, null, null, "Debe seleccionar")]
    [InlineData("N/A", null, null, "Debe seleccionar")]
    [InlineData("PYME", null, "B12345678", "razón social")]
    [InlineData("PYME", "Empresa SL", null, "CIF")]
    [InlineData("Autonomo", null, null, null)]
    [InlineData("Autónomo", null, null, null)]
    [InlineData("pyme", "Empresa SL", "B12345678", null)]
    public void TipoUsuarioExigeDatosDePymeYAceptaAutonomo(string? tipo, string? razonSocial, string? cif, string? errorEsperado)
    {
        var error = Usuario.ValidarTipoEntidad(tipo, razonSocial, cif);

        if (errorEsperado == null) Assert.Null(error);
        else Assert.Contains(errorEsperado, error);
    }

    [Fact]
    public async Task AltaYEdicionDeUsuariosDeEmpresaConservanReservadaLaCuentaGlobal()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var sesion = new EmpresaContext();
        sesion.Establecer(2, esSuperAdmin: true);
        var provider = new DbContextProviderEnMemoria(options, new ConfigurationBuilder().Build(), new HttpContextAccessor(), sesion);
        var servicio = new UsuarioService(provider, NullLogger<UsuarioService>.Instance);
        var administrador = new Usuario { NombreUsuario = " administrador ", Rol = "Administrador" };

        Assert.False((await servicio.CrearAsync(new Usuario { NombreUsuario = " SUPERADMIN " }, "prueba")).exito);
        Assert.True((await servicio.CrearAsync(administrador, "prueba")).exito);
        Assert.Equal("administrador", administrador.NombreUsuario);

        administrador.Nombre = "Nombre actualizado";
        Assert.True((await servicio.ActualizarAsync(administrador)).exito);
        administrador.NombreUsuario = " SuperAdmin ";
        Assert.False((await servicio.ActualizarAsync(administrador)).exito);

        var guardado = await servicio.ObtenerPorIdAsync(administrador.Id);
        Assert.NotNull(guardado);
        Assert.Equal("administrador", guardado.NombreUsuario);
        Assert.Equal("Nombre actualizado", guardado.Nombre);
        Assert.Equal("prueba", guardado.PasswordHash);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AltaDeColaboradorRequiereFirmaAntesDeHabilitar(bool firmaManual)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        using (var admin = new ApplicationDbContext(options, 2, esSuperAdmin: true))
        {
            admin.Empresas.Add(new Empresa { Id = 2, Nombre = "Empresa de prueba" });
            admin.Usuarios.Add(new Usuario { Id = 40, Rol = "Administrador", NombreUsuario = "admin-empresa", Activo = true });
            admin.SaveChanges();
        }

        var sesion = new EmpresaContext();
        sesion.Establecer(2);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new System.Collections.Generic.Dictionary<string, string?> { ["PublicSigning:BaseUrl"] = "http://localhost:5169" }).Build();
        var http = new HttpContextAccessor();
        var provider = new DbContextProviderEnMemoria(options, configuration, http, sesion);
        var usuarios = new UsuarioService(provider, NullLogger<UsuarioService>.Instance);
        var servicio = new FirmaService(provider, new EmailService(provider), new ContractSigningPdfService(provider), configuration, http, NullLogger<FirmaService>.Instance);
        var usuario = new Usuario { NombreUsuario = "colaborador", Nombre = "Nombre", Email = "prueba@example.invalid", Rol = "Colaborador", Activo = true };
        Assert.True((await usuarios.SolicitarAltaAsync(usuario, "prueba123", 40, new(), new(), new())).exito);
        Assert.False(usuario.Activo);
        Assert.Null(await servicio.ObtenerUltimaSolicitudColaboradorAsync(usuario.Id));
        Assert.False((await servicio.CrearSolicitudColaboradorAsync(usuario.Id)).Exito);

        sesion.Establecer(2, esSuperAdmin: true);
        usuario.Activo = true;
        Assert.False((await usuarios.ActualizarAsync(usuario)).exito);
        var pdf = await servicio.GenerarContratoColaboradorManualAsync(usuario.Id);
        Assert.NotNull(pdf);
        Assert.True(pdf.AsSpan().StartsWith("%PDF-"u8));

        var envio = await servicio.CrearSolicitudColaboradorAsync(usuario.Id);
        Assert.False(envio.Exito);
        Assert.Contains("configuración de la empresa", envio.Mensaje);
        Assert.Equal(EstadoSolicitudFirma.ErrorEnvio, (await servicio.ObtenerUltimaSolicitudColaboradorAsync(usuario.Id))?.Estado);

        if (firmaManual)
        {
            Assert.True((await servicio.GuardarContratoColaboradorManualAsync(usuario.Id, "firmado.pdf", pdf)).Exito);
            Assert.Null((await servicio.ObtenerDocumentoColaboradorFirmadoAsync(usuario.Id, manual: false)).Contenido);
            Assert.Equal(pdf, (await servicio.ObtenerDocumentoColaboradorFirmadoAsync(usuario.Id, manual: true)).Contenido);
        }
        else
        {
            var token = new FirmaTokenService().Generar();
            using (var empresa = new ApplicationDbContext(options, 2))
            {
                empresa.SolicitudesFirmaColaboradores.Add(new SolicitudFirmaColaborador
                {
                    UsuarioId = usuario.Id, TokenHash = new FirmaTokenService().Hash(token), DocumentoOriginal = pdf,
                    Estado = EstadoSolicitudFirma.Enviado, FechaCaducidadUtc = DateTime.UtcNow.AddDays(1)
                });
                empresa.SaveChanges();
            }

            sesion.Establecer(0);
            Assert.NotNull(await servicio.ObtenerSolicitudColaboradorAsync(token));
            var firma = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jR5sAAAAASUVORK5CYII=");
            var resultado = await servicio.CompletarFirmaColaboradorAsync(token, firma);
            Assert.True(resultado.Exito, resultado.Mensaje);
            Assert.Equal(0, sesion.EmpresaId);
            sesion.Establecer(2, esSuperAdmin: true);
            Assert.NotNull((await servicio.ObtenerDocumentoColaboradorFirmadoAsync(usuario.Id, manual: false)).Contenido);
        }

        Assert.False((await usuarios.ObtenerPorIdAsync(usuario.Id))!.Activo);
        Assert.True((await usuarios.ActualizarAsync(usuario)).exito);
        Assert.True((await usuarios.ObtenerPorIdAsync(usuario.Id))!.Activo);
    }

    [Fact]
    public async Task PerfilSuperadminEsGlobalYPersisteSinModificarUsuariosDeEmpresa()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var empresa = new ApplicationDbContext(options, 2, esSuperAdmin: true))
        {
            empresa.Usuarios.Add(new Usuario { Id = 20, NombreUsuario = "admin-empresa", Email = "empresa@example.invalid", Rol = "Administrador" });
            empresa.SaveChanges();
        }

        var sesion = new EmpresaContext();
        sesion.Establecer(0, esSuperAdmin: true);
        var provider = new DbContextProviderEnMemoria(options, new ConfigurationBuilder().Build(), new HttpContextAccessor(), sesion);
        var servicio = new PerfilSuperAdminService(provider, NullLogger<PerfilSuperAdminService>.Instance);
        Assert.Null(await servicio.ObtenerAsync());
        var perfil = new PerfilSuperAdmin { Email = " global@example.invalid " };
        Assert.True((await servicio.GuardarAsync(perfil)).exito);

        sesion.Establecer(2, esSuperAdmin: true);
        var guardado = (await servicio.ObtenerAsync())!;
        Assert.Equal("global@example.invalid", guardado.Email);
        guardado.Email = "nuevo@example.invalid";
        Assert.True((await servicio.GuardarAsync(guardado)).exito);

        sesion.Establecer(0, esSuperAdmin: true);
        var nuevaSesion = new PerfilSuperAdminService(provider, NullLogger<PerfilSuperAdminService>.Instance);
        Assert.Equal("nuevo@example.invalid", (await nuevaSesion.ObtenerAsync())!.Email);
        using var verificacion = new ApplicationDbContext(options, 2, esSuperAdmin: true);
        Assert.Single(verificacion.PerfilesSuperAdmin);
        Assert.Null(verificacion.Model.FindEntityType(typeof(PerfilSuperAdmin))!.FindProperty("EmpresaId"));
        Assert.Equal("empresa@example.invalid", Assert.Single(verificacion.Usuarios).Email);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task UsuarioDeEmpresaNoPuedeLeerNiEditarPerfilGlobal(int empresaId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var global = new ApplicationDbContext(options, 0, esSuperAdmin: true))
        {
            global.PerfilesSuperAdmin.Add(new PerfilSuperAdmin { Email = "global@example.invalid" });
            global.SaveChanges();
        }

        var sesion = new EmpresaContext();
        sesion.Establecer(empresaId, esSuperAdmin: false);
        var provider = new DbContextProviderEnMemoria(options, new ConfigurationBuilder().Build(), new HttpContextAccessor(), sesion);
        var servicio = new PerfilSuperAdminService(provider, NullLogger<PerfilSuperAdminService>.Instance);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.ObtenerAsync());
        Assert.False((await servicio.GuardarAsync(new PerfilSuperAdmin { Email = "intruso@example.invalid" })).exito);

        using var empresa = new ApplicationDbContext(options, empresaId);
        Assert.Empty(empresa.PerfilesSuperAdmin);
        empresa.PerfilesSuperAdmin.Update(new PerfilSuperAdmin { Email = "intruso@example.invalid" });
        Assert.Throws<UnauthorizedAccessException>(() => empresa.SaveChanges());
    }

    [Fact]
    public void PerfilGlobalNoPermiteVariasFilasNiEscriturasDesdeContextoPublico()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var global = new ApplicationDbContext(options, 0, esSuperAdmin: true);
        global.PerfilesSuperAdmin.Add(new PerfilSuperAdmin { Email = "global@example.invalid" });
        global.SaveChanges();
        global.PerfilesSuperAdmin.Add(new PerfilSuperAdmin { Id = 2, Email = "global@example.invalid" });
        Assert.Throws<InvalidOperationException>(() => global.SaveChanges());

        using var publico = new ApplicationDbContext(options, autenticacion: true, publico: true);
        Assert.Empty(publico.PerfilesSuperAdmin);
        publico.PerfilesSuperAdmin.Add(new PerfilSuperAdmin { Email = "publico@example.invalid" });
        Assert.Throws<UnauthorizedAccessException>(() => publico.SaveChanges());
    }

    [Theory]
    [InlineData("")]
    [InlineData("correo-invalido")]
    public async Task PerfilGlobalRechazaCorreoInvalido(string email)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var sesion = new EmpresaContext();
        sesion.Establecer(0, esSuperAdmin: true);
        var provider = new DbContextProviderEnMemoria(options, new ConfigurationBuilder().Build(), new HttpContextAccessor(), sesion);
        var servicio = new PerfilSuperAdminService(provider, NullLogger<PerfilSuperAdminService>.Instance);
        var resultado = await servicio.GuardarAsync(new PerfilSuperAdmin { Email = email });
        Assert.False(resultado.exito);
        Assert.NotEmpty(resultado.mensaje);
        Assert.Null(await servicio.ObtenerAsync());
    }

    [Fact]
    public async Task AvisoDeSolicitudLeeCorreoGlobalActualSinUsarCorreosDeEmpresa()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using (var global = new ApplicationDbContext(options, 0, esSuperAdmin: true))
        {
            global.Empresas.AddRange(new Empresa { Id = 1, Nombre = "Empresa A" }, new Empresa { Id = 2, Nombre = "Empresa B" });
            global.SaveChanges();
        }
        using (var empresa = new ApplicationDbContext(options, 1, esSuperAdmin: true))
        {
            empresa.Usuarios.Add(new Usuario { NombreUsuario = "admin-a", Email = "empresa@example.invalid", Rol = "Administrador" });
            empresa.SaveChanges();
        }

        var sesion = new EmpresaContext();
        sesion.Establecer(1, esSuperAdmin: false);
        var provider = new DbContextProviderEnMemoria(options, new ConfigurationBuilder().Build(), new HttpContextAccessor(), sesion);
        var usuarios = new UsuarioService(provider, NullLogger<UsuarioService>.Instance);
        var sinPerfil = await usuarios.ObtenerDatosAvisoSuperAdminAsync(1);
        Assert.Null(sinPerfil.Email);
        Assert.Equal("Empresa A", sinPerfil.EmpresaNombre);

        sesion.Establecer(0, esSuperAdmin: true);
        var perfil = new PerfilSuperAdminService(provider, NullLogger<PerfilSuperAdminService>.Instance);
        Assert.True((await perfil.GuardarAsync(new PerfilSuperAdmin { Email = "global@example.invalid" })).exito);
        sesion.Establecer(1, esSuperAdmin: false);
        Assert.Equal("global@example.invalid", (await usuarios.ObtenerDatosAvisoSuperAdminAsync(1)).Email);

        sesion.Establecer(0, esSuperAdmin: true);
        Assert.True((await perfil.GuardarAsync(new PerfilSuperAdmin { Email = "nuevo@example.invalid" })).exito);
        sesion.Establecer(2, esSuperAdmin: false);
        var avisoOtraEmpresa = await usuarios.ObtenerDatosAvisoSuperAdminAsync(2);
        Assert.Equal("nuevo@example.invalid", avisoOtraEmpresa.Email);
        Assert.Equal("Empresa B", avisoOtraEmpresa.EmpresaNombre);
    }

    private sealed class DbContextProviderEnMemoria(
        DbContextOptions<ApplicationDbContext> options, IConfiguration configuration, IHttpContextAccessor http, EmpresaContext sesion)
        : DbContextProvider(configuration, http, sesion)
    {
        internal override DbContextOptions<ApplicationDbContext> CrearOpciones() => options;
    }
}