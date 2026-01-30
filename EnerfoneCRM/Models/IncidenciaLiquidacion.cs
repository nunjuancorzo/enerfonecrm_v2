using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("incidencias_liquidacion")]
    public class IncidenciaLiquidacion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("usuario_colaborador_id")]
        public int UsuarioColaboradorId { get; set; }

        [Column("mensaje_colaborador")]
        public string MensajeColaborador { get; set; } = string.Empty;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("respuesta_administrador")]
        public string? RespuestaAdministrador { get; set; }

        [Column("usuario_administrador_id")]
        public int? UsuarioAdministradorId { get; set; }

        [Column("fecha_respuesta")]
        public DateTime? FechaRespuesta { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Respondida

        // Navegación
        [ForeignKey("UsuarioColaboradorId")]
        public virtual Usuario? UsuarioColaborador { get; set; }

        [ForeignKey("UsuarioAdministradorId")]
        public virtual Usuario? UsuarioAdministrador { get; set; }
    }
}
