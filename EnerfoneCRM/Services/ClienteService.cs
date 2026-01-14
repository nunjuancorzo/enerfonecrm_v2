using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class ClienteService
{
    private readonly DbContextProvider _dbContextProvider;

    public ClienteService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<Cliente>> ObtenerTodosAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var clientes = await context.Clientes
            .OrderBy(c => c.TipoCliente)
            .ThenBy(c => c.Nombre)
            .ToListAsync();

        // Cargar nombres de usuarios
        foreach (var cliente in clientes.Where(c => c.IdUsuario.HasValue))
        {
            var usuario = await context.Usuarios.FindAsync(cliente.IdUsuario!.Value);
            if (usuario != null)
            {
                cliente.NombreUsuario = usuario.NombreUsuario;
            }
        }

        return clientes;
    }

    public async Task<List<Cliente>> ObtenerPorUsuarioAsync(int idUsuario)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var clientes = await context.Clientes
            .Where(c => c.IdUsuario == idUsuario)
            .OrderBy(c => c.TipoCliente)
            .ThenBy(c => c.Nombre)
            .ToListAsync();

        // Cargar nombres de usuarios
        foreach (var cliente in clientes.Where(c => c.IdUsuario.HasValue))
        {
            var usuario = await context.Usuarios.FindAsync(cliente.IdUsuario!.Value);
            if (usuario != null)
            {
                cliente.NombreUsuario = usuario.NombreUsuario;
            }
        }

        return clientes;
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var cliente = await context.Clientes.FindAsync(id);
        
        if (cliente != null && cliente.IdUsuario.HasValue)
        {
            var usuario = await context.Usuarios.FindAsync(cliente.IdUsuario!.Value);
            if (usuario != null)
            {
                cliente.NombreUsuario = usuario.NombreUsuario;
            }
        }
        
        return cliente;
    }

    public async Task<(bool exito, string mensaje)> CrearAsync(Cliente cliente)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            // Verificar si ya existe un cliente con el mismo DNI
            if (!string.IsNullOrEmpty(cliente.DniCif))
            {
                var existeDni = await context.Clientes
                    .AnyAsync(c => c.DniCif == cliente.DniCif);
                
                if (existeDni)
                {
                    return (false, "Ya existe un cliente con este DNI");
                }
            }

            cliente.FechaAlta = DateTime.Now;
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
            return (true, "Cliente creado correctamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al crear el cliente: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(Cliente cliente)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            // Verificar si ya existe otro cliente con el mismo DNI
            if (!string.IsNullOrEmpty(cliente.DniCif))
            {
                var existeDni = await context.Clientes
                    .AnyAsync(c => c.DniCif == cliente.DniCif && c.Id != cliente.Id);
                
                if (existeDni)
                {
                    return (false, "Ya existe otro cliente con este DNI");
                }
            }

            context.Clientes.Update(cliente);
            await context.SaveChangesAsync();
            return (true, "Cliente actualizado correctamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar el cliente: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            // Verificar si el cliente tiene contratos asociados
            var tieneContratos = await context.Contratos
                .AnyAsync(c => c.IdCliente == id);
            
            if (tieneContratos)
            {
                return (false, "No se puede eliminar el cliente porque tiene contratos asociados");
            }

            var cliente = await context.Clientes.FindAsync(id);
            if (cliente == null)
                return (false, "Cliente no encontrado");

            context.Clientes.Remove(cliente);
            await context.SaveChangesAsync();
            return (true, "Cliente eliminado correctamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar el cliente: {ex.Message}");
        }
    }

    public async Task<List<string>> ObtenerComercialesAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Clientes
            .Where(c => !string.IsNullOrEmpty(c.Comercial))
            .Select(c => c.Comercial!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<int> ObtenerConteoContratosAsync(int clienteId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Contratos
            .Where(c => c.IdCliente == clienteId)
            .CountAsync();
    }
}
