using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("noticias_imagenes")]
public class NoticiaImagen
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("mensaje_id")]
    [Required]
    public int MensajeId { get; set; }

    [Column("imagen_url")]
    [Required]
    [StringLength(500)]
    public string ImagenUrl { get; set; } = string.Empty;

    [Column("orden")]
    public int Orden { get; set; } = 0;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [ForeignKey("MensajeId")]
    public MensajeBienvenida? Mensaje { get; set; }
}
