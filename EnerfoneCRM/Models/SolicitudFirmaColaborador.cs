using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("solicitudes_firma_colaborador")]
public class SolicitudFirmaColaborador
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("usuario_id")] public int UsuarioId { get; set; }
    [Required, StringLength(36), Column("proceso_id")] public string ProcesoId { get; set; } = string.Empty;
    [Required, StringLength(64), Column("token_hash")] public string TokenHash { get; set; } = string.Empty;
    [Column("fecha_creacion_utc")] public DateTime FechaCreacionUtc { get; set; }
    [Column("fecha_caducidad_utc")] public DateTime FechaCaducidadUtc { get; set; }
    [Column("fecha_firma_utc")] public DateTime? FechaFirmaUtc { get; set; }
    [Required, StringLength(30), Column("estado")] public string Estado { get; set; } = EstadoSolicitudFirma.Enviado;
    [Required, StringLength(255), Column("email_destinatario")] public string EmailDestinatario { get; set; } = string.Empty;
    [Column("nombre_destinatario"), StringLength(255)] public string? NombreDestinatario { get; set; }
    [Required, Column("documento_original")] public byte[] DocumentoOriginal { get; set; } = Array.Empty<byte>();
    [Column("documento_firmado")] public byte[]? DocumentoFirmado { get; set; }
    [Column("firma_imagen")] public byte[]? FirmaImagen { get; set; }
    [StringLength(64), Column("hash_documento_original")] public string? HashDocumentoOriginal { get; set; }
    [StringLength(64), Column("hash_documento_firmado")] public string? HashDocumentoFirmado { get; set; }
    [StringLength(64), Column("ip_firma")] public string? IpFirma { get; set; }
    [StringLength(1000), Column("user_agent_firma")] public string? UserAgentFirma { get; set; }
}