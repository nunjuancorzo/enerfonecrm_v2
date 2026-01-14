using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("mensajes_bienvenida")]
public class MensajeBienvenida
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("titulo")]
    [Required]
    [StringLength(255)]
    public string Titulo { get; set; } = string.Empty;

    [Column("contenido")]
    [Required]
    public string Contenido { get; set; } = string.Empty;

    [Column("imagen_url")]
    [StringLength(500)]
    public string? ImagenUrl { get; set; }

    [Column("fecha_inicio")]
    [Required]
    public DateTime FechaInicio { get; set; }

    [Column("fecha_fin")]
    public DateTime? FechaFin { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("prioridad")]
    public int Prioridad { get; set; } = 0;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("usuario_creacion_id")]
    public int? UsuarioCreacionId { get; set; }

    [ForeignKey("UsuarioCreacionId")]
    public Usuario? UsuarioCreacion { get; set; }
}
