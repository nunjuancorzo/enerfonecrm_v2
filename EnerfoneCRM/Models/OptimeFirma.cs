using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("optime_firmas")]
public class OptimeFirma
{
    public const string TipoContrato = "Contrato";
    public const string TipoAdenda = "Adenda";
    public const string TipoCodigoBuenasPracticas = "CBP";

    public static readonly string[] Tipos = { TipoContrato, TipoAdenda, TipoCodigoBuenasPracticas };

    public static string Titulo(string tipo) => tipo switch
    {
        TipoContrato => "Contrato de colaboración mercantil OPTIME + LOPD",
        TipoAdenda => "Adenda comercial - Adhesión del Punto de Venta (Mayorista)",
        TipoCodigoBuenasPracticas => "Aceptación del Código de Buenas Prácticas",
        _ => tipo
    };

    [Key, Column("id")] public int Id { get; set; }
    [Column("solicitud_id")] public int SolicitudId { get; set; }
    [Required, StringLength(20), Column("tipo_documento")] public string TipoDocumento { get; set; } = string.Empty;
    [Required, StringLength(36), Column("proceso_id")] public string ProcesoId { get; set; } = string.Empty;
    [Required, StringLength(64), Column("token_hash")] public string TokenHash { get; set; } = string.Empty;
    [Column("fecha_creacion_utc")] public DateTime FechaCreacionUtc { get; set; }
    [Column("fecha_caducidad_utc")] public DateTime FechaCaducidadUtc { get; set; }
    [Column("fecha_firma_utc")] public DateTime? FechaFirmaUtc { get; set; }
    [Required, StringLength(30), Column("estado")] public string Estado { get; set; } = EstadoSolicitudFirma.Enviado;
    [Required, StringLength(255), Column("email_destinatario")] public string EmailDestinatario { get; set; } = string.Empty;
    [StringLength(255), Column("nombre_destinatario")] public string? NombreDestinatario { get; set; }
    [Required, Column("documento_original")] public byte[] DocumentoOriginal { get; set; } = Array.Empty<byte>();
    [Column("documento_firmado")] public byte[]? DocumentoFirmado { get; set; }
    [Column("firma_imagen")] public byte[]? FirmaImagen { get; set; }
    [StringLength(64), Column("hash_documento_original")] public string? HashDocumentoOriginal { get; set; }
    [StringLength(64), Column("hash_documento_firmado")] public string? HashDocumentoFirmado { get; set; }
    [StringLength(64), Column("ip_firma")] public string? IpFirma { get; set; }
    [StringLength(1000), Column("user_agent_firma")] public string? UserAgentFirma { get; set; }
    [StringLength(500), Column("error_envio")] public string? ErrorEnvio { get; set; }
}
