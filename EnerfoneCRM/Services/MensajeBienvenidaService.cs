using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace EnerfoneCRM.Services;

public class MensajeBienvenidaService
{
    private readonly DbContextProvider _dbContextProvider;
    private readonly IJSRuntime _jsRuntime;

    public MensajeBienvenidaService(DbContextProvider dbContextProvider, IJSRuntime jsRuntime)
    {
        _dbContextProvider = dbContextProvider;
        _jsRuntime = jsRuntime;
    }

    // Obtener el mensaje activo de mayor prioridad
    public async Task<MensajeBienvenida?> ObtenerMensajeActivoAsync()
    {
        using var context = _dbContextProvider.CreateDbContext();
        var fechaActual = DateTime.Now;

        return await context.MensajesBienvenida
            .Where(m => m.Activo 
                && m.FechaInicio <= fechaActual 
                && (m.FechaFin == null || m.FechaFin >= fechaActual))
            .OrderByDescending(m => m.Prioridad)
            .ThenByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    // Verificar si el mensaje ya fue visto hoy
    public async Task<bool> MensajeVistoHoyAsync(int mensajeId)
    {
        try
        {
            var fechaVisto = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", 
                $"popup_visto_{mensajeId}");
            
            if (string.IsNullOrEmpty(fechaVisto))
                return false;

            var fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");
            return fechaVisto == fechaHoy;
        }
        catch
        {
            return false;
        }
    }

    // Obtener todos los mensajes
    public async Task<List<MensajeBienvenida>> ObtenerTodosLosAsAsync()
    {
        using var context = _dbContextProvider.CreateDbContext();
        return await context.MensajesBienvenida
            .Include(m => m.UsuarioCreacion)
            .OrderByDescending(m => m.FechaCreacion)
            .ToListAsync();
    }

    // Crear un nuevo mensaje
    public async Task<bool> CrearMensajeAsync(MensajeBienvenida mensaje)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        try
        {
            mensaje.FechaCreacion = DateTime.Now;
            context.MensajesBienvenida.Add(mensaje);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Actualizar un mensaje existente
    public async Task<bool> ActualizarMensajeAsync(MensajeBienvenida mensaje)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        try
        {
            context.MensajesBienvenida.Update(mensaje);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Eliminar un mensaje
    public async Task<bool> EliminarMensajeAsync(int id)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        try
        {
            var mensaje = await context.MensajesBienvenida.FindAsync(id);
            if (mensaje == null)
                return false;

            context.MensajesBienvenida.Remove(mensaje);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Obtener un mensaje por ID
    public async Task<MensajeBienvenida?> ObtenerMensajePorIdAsync(int id)
    {
        using var context = _dbContextProvider.CreateDbContext();
        return await context.MensajesBienvenida
            .Include(m => m.UsuarioCreacion)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // Activar/Desactivar mensaje
    public async Task<bool> CambiarEstadoMensajeAsync(int id, bool activo)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        try
        {
            var mensaje = await context.MensajesBienvenida.FindAsync(id);
            if (mensaje == null)
                return false;

            mensaje.Activo = activo;
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
