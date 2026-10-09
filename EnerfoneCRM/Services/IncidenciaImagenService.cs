using EnerfoneCRM.Models;

namespace EnerfoneCRM.Services;

public class IncidenciaImagenService
{
    private readonly string carpeta;

    public IncidenciaImagenService(IConfiguration configuracion)
    {
        var storage = configuracion.GetValue<string>("StoragePath");
        carpeta = Path.Combine(string.IsNullOrWhiteSpace(storage) ? Path.Combine(Directory.GetCurrentDirectory(), "storage") : storage, "incidencias");
    }

    public async Task<string> GuardarAsync(ImagenIncidenciaAdjunta imagen)
    {
        var validada = ImagenIncidenciaAdjunta.Crear(imagen.Contenido, imagen.Nombre);
        Directory.CreateDirectory(carpeta);
        var nombre = $"inc_{Guid.NewGuid():N}{Path.GetExtension(validada.Nombre)}";
        var ruta = Path.Combine(carpeta, nombre);
        try
        {
            await File.WriteAllBytesAsync(ruta, validada.Contenido);
            return nombre;
        }
        catch
        {
            if (File.Exists(ruta)) File.Delete(ruta);
            throw;
        }
    }

    public void EliminarSinGuardar(string nombre)
    {
        if (nombre != Path.GetFileName(nombre) || !nombre.StartsWith("inc_", StringComparison.Ordinal))
            throw new ArgumentException("Nombre de imagen no válido.");
        var ruta = Path.Combine(carpeta, nombre);
        if (File.Exists(ruta)) File.Delete(ruta);
    }
}