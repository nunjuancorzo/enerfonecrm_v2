using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("tareas_pendientes")]
    public class TareaPendiente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("descripcion")]
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Column("id_usuario_asignado")]
        [Required(ErrorMessage = "Debe asignar la tarea a un usuario")]
        public int IdUsuarioAsignado { get; set; }

        [Column("id_usuario_creador")]
        public int? IdUsuarioCreador { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("fecha_vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Column("prioridad")]
        [MaxLength(20)]
        public string? Prioridad { get; set; } = "Media"; // Alta, Media, Baja

        [Column("estado")]
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Activa"; // Activa, Cerrada

        [Column("notas")]
        public string? Notas { get; set; }

        // Propiedades no mapeadas
        [NotMapped]
        public string? NombreUsuarioAsignado { get; set; }

        [NotMapped]
        public string? NombreUsuarioCreador { get; set; }
    }
}
