using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class TareaPendienteService
{
    private readonly DbContextProvider _dbContextProvider;

    public TareaPendienteService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<TareaPendiente>> ObtenerTodasAsync(int? idUsuario = null, string? estado = null)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var query = context.TareasPendientes.AsQueryable();

        // Filtrar por usuario si se especifica
        if (idUsuario.HasValue)
        {
            query = query.Where(t => t.IdUsuarioAsignado == idUsuario.Value);
        }

        // Filtrar por estado si se especifica
        if (!string.IsNullOrEmpty(estado))
        {
            query = query.Where(t => t.Estado == estado);
        }

        var tareas = await query
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();

        // Cargar nombres de usuarios
        foreach (var tarea in tareas)
        {
            if (tarea.IdUsuarioAsignado > 0)
            {
                var usuarioAsignado = await context.Usuarios.FindAsync(tarea.IdUsuarioAsignado);
                if (usuarioAsignado != null)
                {
                    tarea.NombreUsuarioAsignado = usuarioAsignado.NombreUsuario;
                }
            }

            if (tarea.IdUsuarioCreador.HasValue)
            {
                var usuarioCreador = await context.Usuarios.FindAsync(tarea.IdUsuarioCreador.Value);
                if (usuarioCreador != null)
                {
                    tarea.NombreUsuarioCreador = usuarioCreador.NombreUsuario;
                }
            }
        }

        return tareas;
    }

    public async Task<TareaPendiente?> ObtenerPorIdAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var tarea = await context.TareasPendientes.FindAsync(id);

        if (tarea != null)
        {
            if (tarea.IdUsuarioAsignado > 0)
            {
                var usuarioAsignado = await context.Usuarios.FindAsync(tarea.IdUsuarioAsignado);
                if (usuarioAsignado != null)
                {
                    tarea.NombreUsuarioAsignado = usuarioAsignado.NombreUsuario;
                }
            }

            if (tarea.IdUsuarioCreador.HasValue)
            {
                var usuarioCreador = await context.Usuarios.FindAsync(tarea.IdUsuarioCreador.Value);
                if (usuarioCreador != null)
                {
                    tarea.NombreUsuarioCreador = usuarioCreador.NombreUsuario;
                }
            }
        }

        return tarea;
    }

    public async Task<bool> CrearAsync(TareaPendiente tarea)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            tarea.FechaCreacion = DateTime.Now;
            tarea.Estado = "Activa";
            
            context.TareasPendientes.Add(tarea);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear tarea: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ActualizarAsync(TareaPendiente tarea)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            context.TareasPendientes.Update(tarea);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar tarea: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CambiarEstadoAsync(int id, string nuevoEstado)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var tarea = await context.TareasPendientes.FindAsync(id);
            
            if (tarea == null)
                return false;

            tarea.Estado = nuevoEstado;
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar estado de tarea: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EliminarAsync(int id)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var tarea = await context.TareasPendientes.FindAsync(id);
            
            if (tarea == null)
                return false;

            context.TareasPendientes.Remove(tarea);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar tarea: {ex.Message}");
            return false;
        }
    }

    public async Task<int> ContarTareasActivasAsync(int idUsuario)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.TareasPendientes
            .CountAsync(t => t.IdUsuarioAsignado == idUsuario && t.Estado == "Activa");
    }
}
