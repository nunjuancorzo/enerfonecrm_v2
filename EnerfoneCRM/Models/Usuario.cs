using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("idusuarios")]
    public int Id { get; set; }

    [Column("username")]
    [StringLength(45)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Column("nombre")]
    [StringLength(100)]
    public string? Nombre { get; set; }

    [Column("apellidos")]
    [StringLength(100)]
    public string? Apellidos { get; set; }

    [Column("direccion")]
    [StringLength(255)]
    public string? Direccion { get; set; }

    [Column("codigo_postal")]
    [StringLength(10)]
    public string? CodigoPostal { get; set; }

    [Column("localidad")]
    [StringLength(100)]
    public string? Localidad { get; set; }

    [Column("email")]
    [StringLength(45)]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    [StringLength(50)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("rol")]
    [StringLength(45)]
    public string Rol { get; set; } = "Usuario";

    [Column("comercializadora")]
    [StringLength(255)]
    public string? Comercializadora { get; set; }

    [Column("comision")]
    public decimal Comision { get; set; } = 0.00m;

    [Column("activo")]
    public bool Activo { get; set; } = false;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Propiedades calculadas para compatibilidad con la UI
    [NotMapped]
    public DateTime? UltimoAcceso { get; set; }

    [NotMapped]
    public int TotalContratos { get; set; }
}
