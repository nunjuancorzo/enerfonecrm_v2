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
}
