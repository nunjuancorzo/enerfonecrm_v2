using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("servicios")]
public class Servicio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("tipo")]
    [StringLength(50)]
    [Required]
    public string Tipo { get; set; } = string.Empty;

    [Column("nombreServicio")]
    [StringLength(100)]
    [Required]
    public string NombreServicio { get; set; } = string.Empty;

    [Column("precio")]
    [StringLength(50)]
    [Required]
    public string Precio { get; set; } = string.Empty;

    [Column("empresa")]
    [StringLength(100)]
    public string? Empresa { get; set; }
}
