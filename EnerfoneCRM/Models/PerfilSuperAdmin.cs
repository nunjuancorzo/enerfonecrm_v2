using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("perfil_superadmin")]
public class PerfilSuperAdmin
{
    [Key]
    [Column("id")]
    public int Id { get; set; } = 1;

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = "Super";

    [Required(ErrorMessage = "Los apellidos son requeridos")]
    [StringLength(100)]
    [Column("apellidos")]
    public string Apellidos { get; set; } = "Admin";

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(254)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("telefono")]
    public string? Telefono { get; set; }

    [StringLength(255)]
    [Column("direccion")]
    public string? Direccion { get; set; }

    [StringLength(10)]
    [Column("codigo_postal")]
    public string? CodigoPostal { get; set; }

    [StringLength(100)]
    [Column("localidad")]
    public string? Localidad { get; set; }
}
