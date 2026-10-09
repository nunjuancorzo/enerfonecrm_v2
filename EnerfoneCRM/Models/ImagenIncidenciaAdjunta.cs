using System.Text;

namespace EnerfoneCRM.Models;

public record ImagenIncidenciaAdjunta(byte[] Contenido, string Nombre, string TipoMime)
{
    public const int MaximoBytes = 20 * 1024 * 1024;

    public static ImagenIncidenciaAdjunta Crear(byte[] contenido, string nombre)
    {
        if (contenido.Length == 0 || contenido.Length > MaximoBytes)
            throw new ArgumentException("La imagen está vacía o supera los 20 MB.");
        string extension;
        string mime;
        if (contenido.Length >= 8 && contenido.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            (extension, mime) = (".png", "image/png");
        else if (contenido.Length >= 3 && contenido[0] == 255 && contenido[1] == 216 && contenido[2] == 255)
            (extension, mime) = (".jpg", "image/jpeg");
        else if (contenido.Length >= 6 && (Encoding.ASCII.GetString(contenido, 0, 6) is "GIF87a" or "GIF89a"))
            (extension, mime) = (".gif", "image/gif");
        else if (contenido.Length >= 14 && contenido[0] == 'B' && contenido[1] == 'M')
            (extension, mime) = (".bmp", "image/bmp");
        else if (contenido.Length >= 12 && Encoding.ASCII.GetString(contenido, 0, 4) == "RIFF" && Encoding.ASCII.GetString(contenido, 8, 4) == "WEBP")
            (extension, mime) = (".webp", "image/webp");
        else throw new ArgumentException("Formato de imagen no admitido. Usa PNG, JPEG, GIF, BMP o WebP.");
        var baseNombre = Path.GetFileNameWithoutExtension(nombre);
        if (string.IsNullOrWhiteSpace(baseNombre)) baseNombre = "captura";
        if (baseNombre.Length > 100) baseNombre = baseNombre[..100];
        return new ImagenIncidenciaAdjunta(contenido, baseNombre + extension, mime);
    }
}