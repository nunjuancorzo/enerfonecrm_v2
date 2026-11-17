using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnerfoneCRM.Services;

public class DatosPruebaService
{
    private readonly DbContextProvider _contextProvider;

    public DatosPruebaService(DbContextProvider contextProvider)
    {
        _contextProvider = contextProvider;
    }

    public async Task<(bool success, string message)> GenerarDatosPruebaAsync(int empresaId)
    {
        var mensajes = new List<string>();
        try
        {
            mensajes.Add($"[DEBUG] Inicio - empresaId: {empresaId}");
            
            using (var context = _contextProvider.CreatePublicDbContext())
            {
                mensajes.Add("[DEBUG] Contexto público creado");
                
                // Verificar que la empresa existe y es empresa de prueba
                var empresa = await context.Empresas.FindAsync(empresaId);
                if (empresa == null)
                {
                    mensajes.Add($"[DEBUG] Empresa no encontrada con ID: {empresaId}");
                    return (false, string.Join("\n", mensajes));
                }
                
                mensajes.Add($"[DEBUG] Empresa encontrada: {empresa.Nombre}, EsEmpresaPrueba: {empresa.EsEmpresaPrueba}");
                
                if (!empresa.EsEmpresaPrueba)
                    return (false, $"Esta empresa no está marcada como empresa de prueba. EsEmpresaPrueba={empresa.EsEmpresaPrueba}");

                // Limpiar datos de prueba existentes
                mensajes.Add("Limpiando datos de prueba existentes...");
                await LimpiarDatosPruebaAsync(context, empresaId);
                mensajes.Add("✓ Limpieza completada");

                int tablasConDatos = 0;

                // Generar datos en orden de dependencias
                mensajes.Add("Generando usuarios...");
                var resUsuarios = await GenerarDatosUsuariosAsync(context, empresaId);
                mensajes.Add($"[DEBUG] Usuarios - success: {resUsuarios.success}, msg: {resUsuarios.message}");
                if (resUsuarios.success) { tablasConDatos++; mensajes.Add($"✓ {resUsuarios.message}"); } else mensajes.Add($"✗ Usuarios: {resUsuarios.message}");
                
                mensajes.Add("Generando comercializadoras...");
                var resComercializadoras = await GenerarDatosComercializadorasAsync(context);
                mensajes.Add($"[DEBUG] Comercializadoras - success: {resComercializadoras.success}, msg: {resComercializadoras.message}");
                if (resComercializadoras.success) { tablasConDatos++; mensajes.Add($"✓ {resComercializadoras.message}"); } else mensajes.Add($"✗ Comercializadoras: {resComercializadoras.message}");
                
                mensajes.Add("Generando operadoras...");
                var resOperadoras = await GenerarDatosOperadorasAsync(context);
                mensajes.Add($"[DEBUG] Operadoras - success: {resOperadoras.success}");
                if (resOperadoras.success) { tablasConDatos++; mensajes.Add($"✓ {resOperadoras.message}"); } else mensajes.Add($"✗ Operadoras: {resOperadoras.message}");
                
                mensajes.Add("Generando empresas de alarmas...");
                var resAlarmas = await GenerarDatosEmpresasAlarmaAsync(context);
                mensajes.Add($"[DEBUG] Alarmas - success: {resAlarmas.success}");
                if (resAlarmas.success) { tablasConDatos++; mensajes.Add($"✓ {resAlarmas.message}"); } else mensajes.Add($"✗ Alarmas: {resAlarmas.message}");
                
                mensajes.Add("Generando tarifas...");
                var resTarifas = await GenerarDatosTarifasAsync(context);
                mensajes.Add($"[DEBUG] Tarifas - success: {resTarifas.success}");
                if (resTarifas.success) { tablasConDatos++; mensajes.Add($"✓ {resTarifas.message}"); } else mensajes.Add($"✗ Tarifas: {resTarifas.message}");
                
                mensajes.Add("Generando servicios...");
                var resServicios = await GenerarDatosServiciosAsync(context);
                mensajes.Add($"[DEBUG] Servicios - success: {resServicios.success}");
                if (resServicios.success) { tablasConDatos++; mensajes.Add($"✓ {resServicios.message}"); } else mensajes.Add($"✗ Servicios: {resServicios.message}");
                
                mensajes.Add("Generando clientes...");
                var resClientes = await GenerarDatosClientesAsync(context);
                mensajes.Add($"[DEBUG] Clientes - success: {resClientes.success}");
                if (resClientes.success) { tablasConDatos++; mensajes.Add($"✓ {resClientes.message}"); } else mensajes.Add($"✗ Clientes: {resClientes.message}");

                var mensajeFinal = string.Join("\n", mensajes);
                mensajes.Add($"[DEBUG] Fin - tablasConDatos: {tablasConDatos}");
                return (true, $"{mensajeFinal}\n\n✓ Datos de prueba generados en {tablasConDatos} tablas");
            }
        }
        catch (Exception ex)
        {
            mensajes.Add($"\n❌ EXCEPCIÓN: {ex.GetType().Name}");
            mensajes.Add($"❌ Mensaje: {ex.Message}");
            if (ex.InnerException != null)
                mensajes.Add($"❌ InnerException: {ex.InnerException.Message}");
            return (false, string.Join("\n", mensajes));
        }
    }

    private async Task<(bool success, string message)> GenerarDatosUsuariosAsync(ApplicationDbContext context, int empresaId)
    {
        try
        {
            var usuarios = new[]
            {
                new Usuario { NombreUsuario = "admin_prueba", Nombre = "Admin Prueba", Email = "admin@prueba.test", Rol = "Administrador", EmpresaId = empresaId, FechaCreacion = DateTime.UtcNow, Activo = true, PasswordHash = "dummy" },
                new Usuario { NombreUsuario = "comercial_prueba", Nombre = "Comercial Prueba", Email = "comercial@prueba.test", Rol = "Comercial", EmpresaId = empresaId, FechaCreacion = DateTime.UtcNow, Activo = true, PasswordHash = "dummy" },
                new Usuario { NombreUsuario = "proveedor_prueba", Nombre = "Proveedor Prueba", Email = "proveedor@prueba.test", Rol = "Proveedor", EmpresaId = empresaId, FechaCreacion = DateTime.UtcNow, Activo = true, PasswordHash = "dummy" }
            };

            await context.Usuarios.AddRangeAsync(usuarios);
            await context.SaveChangesAsync();
            return (true, "3 usuarios creados");
        }
        catch (Exception ex) 
        { 
            return (false, $"Error: {ex.Message}"); 
        }
    }

    private async Task<(bool success, string message)> GenerarDatosComercializadorasAsync(ApplicationDbContext context)
    {
        try
        {
            var comercializadoras = new[] {
                new Comercializadora { Nombre = "Comercializadora Prueba 1", Activo = true },
                new Comercializadora { Nombre = "Comercializadora Prueba 2", Activo = true }
            };

            await context.Comercializadoras.AddRangeAsync(comercializadoras);
            await context.SaveChangesAsync();
            return (true, "2 comercializadoras creadas");
        }
        catch (Exception ex) 
        { 
            return (false, $"Error: {ex.Message}"); 
        }
    }

    private async Task<(bool success, string message)> GenerarDatosOperadorasAsync(ApplicationDbContext context)
    {
        try
        {
            var operadoras = new[] {
                new Operadora { Nombre = "Operadora Prueba 1", Activo = true },
                new Operadora { Nombre = "Operadora Prueba 2", Activo = true }
            };

            await context.Operadoras.AddRangeAsync(operadoras);
            await context.SaveChangesAsync();
            return (true, "2 operadoras creadas");
        }
        catch (Exception ex) 
        { 
            return (false, $"Error: {ex.Message}"); 
        }
    }

    private async Task<(bool success, string message)> GenerarDatosEmpresasAlarmaAsync(ApplicationDbContext context)
    {
        try
        {
            var empresasAlarma = new[] {
                new EmpresaAlarma { Nombre = "Empresa Alarma Prueba 1", Activo = true },
                new EmpresaAlarma { Nombre = "Empresa Alarma Prueba 2", Activo = true }
            };

            await context.EmpresasAlarmas.AddRangeAsync(empresasAlarma);
            await context.SaveChangesAsync();
            return (true, "2 empresas de alarmas creadas");
        }
        catch (Exception ex) 
        { 
            return (false, $"Error: {ex.Message}"); 
        }
    }

    private async Task<(bool success, string message)> GenerarDatosTarifasAsync(ApplicationDbContext context)
    {
        try
        {
            await context.TarifasEnergia.AddRangeAsync(
                new TarifaEnergia { Nombre = "Tarifa Energía Prueba 1", Activa = true },
                new TarifaEnergia { Nombre = "Tarifa Energía Prueba 2", Activa = true }
            );

            await context.TarifasLuz.AddAsync(new TarifaLuz { Nombre = "Tarifa Luz Prueba 1", Activa = true });

            await context.TarifasGas.AddAsync(new TarifaGas { Nombre = "Tarifa Gas Prueba 1", Activa = true });

            await context.SaveChangesAsync();
            return (true, "3 tarifas creadas");
        }
        catch (Exception ex) 
        { 
            return (false, $"Error: {ex.Message}"); 
        }
    }

    private async Task<(bool success, string message)> GenerarDatosServiciosAsync(ApplicationDbContext context)
    {
        try
        {
            var servicios = new[] {
                new Servicio { Tipo = "Prueba", NombreServicio = "Servicio Prueba 1", Precio = "0" },
                new Servicio { Tipo = "Prueba", NombreServicio = "Servicio Prueba 2", Precio = "0" },
                new Servicio { Tipo = "Prueba", NombreServicio = "Servicio Prueba 3", Precio = "0" }
            };

            await context.Servicios.AddRangeAsync(servicios);
            await context.SaveChangesAsync();
            return (true, "3 servicios creados");
        }
        catch (Exception ex) 
        { 
            return (false, $"Error: {ex.Message}"); 
        }
    }

    private async Task<(bool success, string message)> GenerarDatosClientesAsync(ApplicationDbContext context)
    {
        try
        {
            var clientes = new[] {
                new Cliente { Nombre = "Cliente Prueba 1", Apellidos = "García", Email = "cliente1@prueba.test", Telefono = "600000001", TipoCliente = "Particular", FechaAlta = DateTime.UtcNow },
                new Cliente { Nombre = "Cliente Prueba 2", Apellidos = "López", Email = "cliente2@prueba.test", Telefono = "600000002", TipoCliente = "Particular", FechaAlta = DateTime.UtcNow },
                new Cliente { Nombre = "Cliente Prueba 3", Apellidos = "Martínez", Email = "cliente3@prueba.test", Telefono = "600000003", TipoCliente = "Pyme", FechaAlta = DateTime.UtcNow }
            };

            await context.Clientes.AddRangeAsync(clientes);
            await context.SaveChangesAsync();
            return (true, "3 clientes creados");
        }
        catch (Exception ex) 
        { 
            return (false, $"Error: {ex.Message}"); 
        }
    }

    private async Task LimpiarDatosPruebaAsync(ApplicationDbContext context, int empresaId)
    {
        try
        {
            // Eliminar clientes de prueba
            var clientesPrueba = await context.Clientes.Where(c => c.Email != null && c.Email.Contains("@prueba.test")).ToListAsync();
            if (clientesPrueba.Any())
            {
                context.Clientes.RemoveRange(clientesPrueba);
                await context.SaveChangesAsync();
            }

            // Eliminar servicios de prueba
            var serviciosPrueba = await context.Servicios.Where(s => s.NombreServicio.Contains("Prueba")).ToListAsync();
            if (serviciosPrueba.Any())
            {
                context.Servicios.RemoveRange(serviciosPrueba);
                await context.SaveChangesAsync();
            }

            // Eliminar tarifas de prueba
            var tarifasEnergiaPrueba = await context.TarifasEnergia.Where(t => t.Nombre.Contains("Prueba")).ToListAsync();
            if (tarifasEnergiaPrueba.Any()) context.TarifasEnergia.RemoveRange(tarifasEnergiaPrueba);

            var tarifasLuzPrueba = await context.TarifasLuz.Where(t => t.Nombre.Contains("Prueba")).ToListAsync();
            if (tarifasLuzPrueba.Any()) context.TarifasLuz.RemoveRange(tarifasLuzPrueba);

            var tarifasGasPrueba = await context.TarifasGas.Where(t => t.Nombre.Contains("Prueba")).ToListAsync();
            if (tarifasGasPrueba.Any()) context.TarifasGas.RemoveRange(tarifasGasPrueba);

            if (tarifasEnergiaPrueba.Any() || tarifasLuzPrueba.Any() || tarifasGasPrueba.Any())
                await context.SaveChangesAsync();

            // Eliminar empresas de alarmas de prueba
            var empresasAlarmasPrueba = await context.EmpresasAlarmas.Where(e => e.Nombre.Contains("Prueba")).ToListAsync();
            if (empresasAlarmasPrueba.Any())
            {
                context.EmpresasAlarmas.RemoveRange(empresasAlarmasPrueba);
                await context.SaveChangesAsync();
            }

            // Eliminar operadoras de prueba
            var operadorasPrueba = await context.Operadoras.Where(o => o.Nombre.Contains("Prueba")).ToListAsync();
            if (operadorasPrueba.Any())
            {
                context.Operadoras.RemoveRange(operadorasPrueba);
                await context.SaveChangesAsync();
            }

            // Eliminar comercializadoras de prueba
            var comercializadorasPrueba = await context.Comercializadoras.Where(c => c.Nombre.Contains("Prueba")).ToListAsync();
            if (comercializadorasPrueba.Any())
            {
                context.Comercializadoras.RemoveRange(comercializadorasPrueba);
                await context.SaveChangesAsync();
            }

            // Eliminar usuarios de prueba de esta empresa
            var usuariosPrueba = await context.Usuarios.Where(u => u.EmpresaId == empresaId && u.Email.Contains("@prueba.test")).ToListAsync();
            if (usuariosPrueba.Any())
            {
                context.Usuarios.RemoveRange(usuariosPrueba);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error limpiando datos de prueba: {ex.Message}");
        }
    }
}

