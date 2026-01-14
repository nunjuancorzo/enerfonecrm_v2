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
}
