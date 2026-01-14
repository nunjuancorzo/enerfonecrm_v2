using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EnerfoneCRM.Services;

public class UsuarioService
{
    private readonly DbContextProvider _dbContextProvider;

    public UsuarioService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var usuarios = await context.Usuarios
            .OrderByDescending(u => u.FechaCreacion)
            .ToListAsync();

        // Cargar el conteo de contratos para cada usuario
        // Los contratos tienen el campo "comercial" que contiene el nombre del usuario
        foreach (var usuario in usuarios)
        {
            // Contar todos los contratos donde el campo comercial coincide con el nombre de usuario
            usuario.TotalContratos = await context.Contratos
                .Where(c => c.Comercial == usuario.NombreUsuario)
                .CountAsync();
        }

        return usuarios;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Usuarios.FindAsync(id);
    }

    public async Task<(bool exito, string mensaje)> CrearAsync(Usuario usuario, string password)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var existeNombreUsuario = await context.Usuarios
            .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario);

        if (existeNombreUsuario)
        {
            return (false, "Ya existe un usuario con este nombre de usuario");
        }

        var existeEmail = await context.Usuarios
            .AnyAsync(u => u.Email == usuario.Email);

        if (existeEmail)
        {
            return (false, "Ya existe un usuario con este email");
        }

        // Contraseña en texto plano (sin hashear)
        usuario.PasswordHash = password;

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return (true, "Usuario creado exitosamente");
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(Usuario usuario)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var existeNombreUsuario = await context.Usuarios
            .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario && u.Id != usuario.Id);

        if (existeNombreUsuario)
        {
            return (false, "Ya existe otro usuario con este nombre de usuario");
        }

        var existeEmail = await context.Usuarios
            .AnyAsync(u => u.Email == usuario.Email && u.Id != usuario.Id);

        if (existeEmail)
        {
            return (false, "Ya existe otro usuario con este email");
        }

        context.Usuarios.Update(usuario);
        await context.SaveChangesAsync();

        return (true, "Usuario actualizado exitosamente");
    }

    // Métodos para gestionar comercializadoras permitidas
    public async Task<List<int>> ObtenerComercializadorasPermitidasAsync(int usuarioId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.UsuarioComercializadoras
            .Where(uc => uc.UsuarioId == usuarioId)
            .Select(uc => uc.ComercializadoraId)
            .ToListAsync();
    }

    public async Task<(bool exito, string mensaje)> ActualizarComercializadorasPermitidasAsync(int usuarioId, List<int> comercializadorasIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Eliminar todas las asignaciones existentes
        var asignacionesExistentes = await context.UsuarioComercializadoras
            .Where(uc => uc.UsuarioId == usuarioId)
            .ToListAsync();
        
        context.UsuarioComercializadoras.RemoveRange(asignacionesExistentes);
        
        // Crear las nuevas asignaciones
        foreach (var comercializadoraId in comercializadorasIds)
        {
            context.UsuarioComercializadoras.Add(new UsuarioComercializadora
            {
                UsuarioId = usuarioId,
                ComercializadoraId = comercializadoraId,
                FechaAsignacion = DateTime.Now
            });
        }
        
        await context.SaveChangesAsync();
        return (true, "Comercializadoras actualizadas exitosamente");
    }

    public async Task<(bool exito, string mensaje)> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNuevo)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var usuario = await context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado");
        }

        // Comparación directa sin hashear
        if (usuario.PasswordHash != passwordActual)
        {
            return (false, "La contraseña actual es incorrecta");
        }

        usuario.PasswordHash = passwordNuevo;
        await context.SaveChangesAsync();

        return (true, "Contraseña actualizada exitosamente");
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var usuario = await context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado");
        }

        context.Usuarios.Remove(usuario);
        await context.SaveChangesAsync();

        return (true, "Usuario eliminado exitosamente");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerificarPassword(string password, string passwordHash)
    {
        var hash = HashPassword(password);
        return hash == passwordHash;
    }
}
