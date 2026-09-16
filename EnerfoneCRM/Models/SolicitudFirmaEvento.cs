using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("solicitudes_firma_eventos")]
public class SolicitudFirmaEvento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("solicitud_firma_id")]
    public int SolicitudFirmaId { get; set; }

    [Required]
    [Column("tipo_evento")]
    [StringLength(50)]
    public string TipoEvento { get; set; } = string.Empty;

    [Column("fecha_hora_utc")]
    public DateTime FechaHoraUtc { get; set; }

    [Column("ip")]
    [StringLength(64)]
    public string? Ip { get; set; }

    [Column("user_agent")]
    [StringLength(1000)]
    public string? UserAgent { get; set; }

    [Column("documento_id")]
    public int? DocumentoId { get; set; }

    [Column("resultado")]
    [StringLength(50)]
    public string? Resultado { get; set; }

    [Column("datos")]
    public string? Datos { get; set; }

    public SolicitudFirma? SolicitudFirma { get; set; }
}