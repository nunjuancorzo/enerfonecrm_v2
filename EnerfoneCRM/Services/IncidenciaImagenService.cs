using EnerfoneCRM.Models;

namespace EnerfoneCRM.Services;

public class IncidenciaImagenService
{
    private readonly IConfiguration _configuration;
    private readonly EmpresaContext _empresa;
    private string carpeta => Path.Combine(_empresa.RutaStorage(_configuration), "incidencias");
    public string UrlImagen(string nombre) => _empresa.UrlStorage(_configuration, $"incidencias/{Uri.EscapeDataString(nombre)}");

    public IncidenciaImagenService(IConfiguration configuracion, EmpresaContext empresa)
    {
        _configuration = configuracion;
        _empresa = empresa;
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