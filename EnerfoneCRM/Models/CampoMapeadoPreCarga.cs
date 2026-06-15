namespace EnerfoneCRM.Models;

/// <summary>
/// Representa un campo mapeado en una plantilla de PreCarga
/// Este modelo no se guarda en BD separado, sino como JSON dentro de PlantillaPreCarga
/// </summary>
public class CampoMapeadoPreCarga
{
    public string Field { get; set; } = string.Empty; // Nombre técnico del campo (ej: "cups", "total_factura")
    public string Label { get; set; } = string.Empty; // Etiqueta visible (ej: "CUPS", "Total Factura")
    public int Page { get; set; } // Número de página (1-based)
    public double X { get; set; } // Posición X relativa (0-1)
    public double Y { get; set; } // Posición Y relativa (0-1)
    public double W { get; set; } // Ancho relativo (0-1)
    public double H { get; set; } // Alto relativo (0-1)
    public string? TipoDato { get; set; } // "texto", "numero", "fecha", "decimal"
    public string? Formato { get; set; } // Para fechas, números, etc.
}
