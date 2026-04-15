namespace EnerfoneCRM.Models;

public class SipsConsultaResult
{
    public SipsResponse? Respuesta { get; init; }

    /// <summary>
    /// True cuando se ha devuelto desde el histórico (BD) o cache en memoria.
    /// </summary>
    public bool DesdeCache { get; init; }

    /// <summary>
    /// Fecha de la consulta original guardada (si viene de histórico) o de la llamada actual.
    /// </summary>
    public DateTime? FechaConsulta { get; init; }

    public int? HistoricoId { get; init; }

    /// <summary>
    /// "API" o "HISTORICO".
    /// </summary>
    public string Fuente { get; init; } = "API";
}
