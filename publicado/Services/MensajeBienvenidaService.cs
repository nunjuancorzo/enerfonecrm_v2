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
            .Include(m => m.Imagenes!.OrderBy(i => i.Orden))
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
            .Include(m => m.Imagenes!.OrderBy(i => i.Orden))
            .OrderByDescending(m => m.FechaCreacion)
            .ToListAsync();
    }

    // Crear un nuevo mensaje
    public async Task<(int id, string? error)> CrearMensajeAsync(MensajeBienvenida mensaje)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        try
        {
            mensaje.FechaCreacion = DateTime.Now;
            context.MensajesBienvenida.Add(mensaje);
            await context.SaveChangesAsync();
            return (mensaje.Id, null); // Devolver el ID generado sin error
        }
        catch (Exception ex)
        {
            var errorMsg = ex.InnerException?.Message ?? ex.Message;
            return (0, errorMsg); // Devolver 0 con el mensaje de error
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
            .Include(m => m.Imagenes!.OrderBy(i => i.Orden))
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

    // Agregar imagen a una noticia
    public async Task<bool> AgregarImagenAsync(NoticiaImagen imagen)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        try
        {
            context.NoticiasImagenes.Add(imagen);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Eliminar imagen de una noticia
    public async Task<bool> EliminarImagenAsync(int imagenId)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        try
        {
            var imagen = await context.NoticiasImagenes.FindAsync(imagenId);
            if (imagen == null)
                return false;

            context.NoticiasImagenes.Remove(imagen);
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Obtener imágenes de una noticia
    public async Task<List<NoticiaImagen>> ObtenerImagenesNoticiaAsync(int mensajeId)
    {
        using var context = _dbContextProvider.CreateDbContext();
        return await context.NoticiasImagenes
            .Where(i => i.MensajeId == mensajeId)
            .OrderBy(i => i.Orden)
            .ToListAsync();
    }

    // Actualizar orden de las imágenes
    public async Task<bool> ActualizarOrdenImagenesAsync(List<NoticiaImagen> imagenes)
    {
        using var context = _dbContextProvider.CreateDbContext();
        
        try
        {
            foreach (var imagen in imagenes)
            {
                var imagenExistente = await context.NoticiasImagenes.FindAsync(imagen.Id);
                if (imagenExistente != null)
                {
                    imagenExistente.Orden = imagen.Orden;
                }
            }
            await context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
