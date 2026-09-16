using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("solicitudes_firma")]
public class SolicitudFirma
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("contrato_id")]
    public int ContratoId { get; set; }

    [Required]
    [Column("proceso_id")]
    [StringLength(36)]
    public string ProcesoId { get; set; } = string.Empty;

    [Required]
    [Column("token_hash")]
    [StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    [Column("fecha_creacion_utc")]
    public DateTime FechaCreacionUtc { get; set; }

    [Column("fecha_caducidad_utc")]
    public DateTime FechaCaducidadUtc { get; set; }

    [Column("fecha_aceptacion_utc")]
    public DateTime? FechaAceptacionUtc { get; set; }

    [Column("fecha_firma_utc")]
    public DateTime? FechaFirmaUtc { get; set; }

    [Required]
    [Column("estado")]
    [StringLength(30)]
    public string Estado { get; set; } = EstadoSolicitudFirma.Pendiente;

    [Required]
    [Column("email_destinatario")]
    [StringLength(255)]
    public string EmailDestinatario { get; set; } = string.Empty;

    [Column("nombre_destinatario")]
    [StringLength(255)]
    public string? NombreDestinatario { get; set; }

    [Column("documento_original_id")]
    public int DocumentoOriginalId { get; set; }

    [Column("documento_firmado_id")]
    public int? DocumentoFirmadoId { get; set; }

    [Column("firma_imagen")]
    public byte[]? FirmaImagen { get; set; }

    [Column("hash_documento_original")]
    [StringLength(64)]
    public string? HashDocumentoOriginal { get; set; }

    [Column("hash_documento_firmado")]
    [StringLength(64)]
    public string? HashDocumentoFirmado { get; set; }

    [Column("ip_firma")]
    [StringLength(64)]
    public string? IpFirma { get; set; }

    [Column("user_agent_firma")]
    [StringLength(1000)]
    public string? UserAgentFirma { get; set; }

    public Contrato? Contrato { get; set; }
}

public static class EstadoSolicitudFirma
{
    public const string Enviado = "Enviado";
    public const string Pendiente = "Pendiente";
    public const string EnProceso = "En proceso";
    public const string Firmado = "Firmado";
    public const string Cancelado = "Cancelado";
    public const string Caducado = "Caducado";
}