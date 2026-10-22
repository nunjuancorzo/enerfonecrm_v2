using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("configuracion_empresa")]
public class ConfiguracionEmpresa
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre_empresa")]
    [Required]
    [StringLength(255)]
    public string NombreEmpresa { get; set; } = string.Empty;

    [Column("cif")]
    [StringLength(20)]
    public string? Cif { get; set; }

    [Column("direccion")]
    [StringLength(255)]
    public string? Direccion { get; set; }

    [Column("codigo_postal")]
    [StringLength(10)]
    public string? CodigoPostal { get; set; }

    [Column("ciudad")]
    [StringLength(100)]
    public string? Ciudad { get; set; }

    [Column("provincia")]
    [StringLength(100)]
    public string? Provincia { get; set; }

    [Column("pais")]
    [StringLength(100)]
    public string? Pais { get; set; } = "España";

    [Column("telefono")]
    [StringLength(20)]
    public string? Telefono { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("web")]
    [StringLength(255)]
    public string? Web { get; set; }

    [Column("logo_url")]
    public string? LogoUrl { get; set; }

    [Column("fecha_actualizacion")]
    public DateTime? FechaActualizacion { get; set; }

    [Column("tema_visual")]
    [StringLength(30)]
    public string TemaVisual { get; set; } = "azul";

    [Column("tema_color_acento")]
    [StringLength(7)]
    public string? TemaColorAcento { get; set; }

    [Column("tema_color_barra")]
    [StringLength(7)]
    public string? TemaColorBarra { get; set; }

    [Column("tema_color_fondo")]
    [StringLength(7)]
    public string? TemaColorFondo { get; set; }

    [Column("tema_visual_anterior")]
    [StringLength(30)]
    public string? TemaVisualAnterior { get; set; }

    [Column("tema_anterior_color_acento")]
    [StringLength(7)]
    public string? TemaAnteriorColorAcento { get; set; }

    [Column("tema_anterior_color_barra")]
    [StringLength(7)]
    public string? TemaAnteriorColorBarra { get; set; }

    [Column("tema_anterior_color_fondo")]
    [StringLength(7)]
    public string? TemaAnteriorColorFondo { get; set; }

    [Column("tema_actualizado_por")]
    [StringLength(255)]
    public string? TemaActualizadoPor { get; set; }

    [Column("tema_actualizado_en")]
    public DateTime? TemaActualizadoEn { get; set; }

    // Configuración SMTP para envío de emails
    [Column("smtp_servidor")]
    [StringLength(255)]
    public string? SmtpServidor { get; set; }

    [Column("smtp_puerto")]
    public int? SmtpPuerto { get; set; } = 587;

    [Column("smtp_usuario")]
    [StringLength(255)]
    public string? SmtpUsuario { get; set; }

    [Column("smtp_password")]
    [StringLength(255)]
    public string? SmtpPassword { get; set; }

    [Column("smtp_usar_ssl")]
    public bool SmtpUsarSsl { get; set; } = true;

    [Column("smtp_email_desde")]
    [StringLength(255)]
    public string? SmtpEmailDesde { get; set; }

    [Column("smtp_nombre_desde")]
    [StringLength(255)]
    public string? SmtpNombreDesde { get; set; }

    // Control de módulos
    [Column("modulo_liquidaciones_activo")]
    public bool ModuloLiquidacionesActivo { get; set; } = true;

    [Column("modulo_sips_activo")]
    public bool ModuloSipsActivo { get; set; } = true;

    [Column("envio_documentos_firma_automatico")]
    public bool EnvioDocumentosFirmaAutomatico { get; set; } = true;

    // Configuración OCR para análisis de facturas
    [Column("ocr_proveedor")]
    [StringLength(100)]
    public string? OcrProveedor { get; set; } = "tesseract"; // azure, openai, google, tesseract

    [Column("ocr_api_key")]
    [StringLength(500)]
    public string? OcrApiKey { get; set; }

    [Column("ocr_endpoint")]
    [StringLength(500)]
    public string? OcrEndpoint { get; set; }

    [Column("ocr_modelo")]
    [StringLength(100)]
    public string? OcrModelo { get; set; }

    [Column("ocr_timeout")]
    public int? OcrTimeout { get; set; } = 30;

    [Column("ocr_proveedor_secundario")]
    [StringLength(100)]
    public string? OcrProveedorSecundario { get; set; }

    [Column("ocr_fallback_automatico")]
    public bool OcrFallbackAutomatico { get; set; } = true;
}
