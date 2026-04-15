using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("incidencias")]
public class Incidencia
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("asunto")]
    [StringLength(200)]
    public string Asunto { get; set; } = string.Empty;

    [Column("tipo_incidencia")]
    [StringLength(50)]
    public string TipoIncidencia { get; set; } = string.Empty;

    [Column("prioridad")]
    [StringLength(20)]
    public string Prioridad { get; set; } = string.Empty;

    [Column("descripcion")]
    [StringLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("nombre_usuario")]
    [StringLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Column("email_usuario")]
    [StringLength(100)]
    public string EmailUsuario { get; set; } = string.Empty;

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En Proceso, Resuelta, Cerrada

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("fecha_actualizacion")]
    public DateTime? FechaActualizacion { get; set; }

    [Column("observaciones_admin")]
    [StringLength(1000)]
    public string? ObservacionesAdmin { get; set; }

    [Column("tiene_imagen")]
    public bool TieneImagen { get; set; } = false;

    [Column("nombre_imagen")]
    [StringLength(200)]
    public string? NombreImagen { get; set; }
}
