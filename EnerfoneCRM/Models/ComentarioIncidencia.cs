using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("comentarios_incidencias")]
public class ComentarioIncidencia
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("incidencia_id")]
    public int IncidenciaId { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("nombre_usuario")]
    [StringLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Column("comentario")]
    [StringLength(2000)]
    public string Comentario { get; set; } = string.Empty;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("email_enviado")]
    public bool EmailEnviado { get; set; } = false;

    // Navegación
    [ForeignKey("IncidenciaId")]
    public Incidencia? Incidencia { get; set; }
}
