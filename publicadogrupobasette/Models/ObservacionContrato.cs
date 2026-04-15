using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("observaciones_contratos")]
    public class ObservacionContrato
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_contrato")]
        public int IdContrato { get; set; }

        [Column("observacion")]
        [StringLength(4000)]
        public string Observacion { get; set; } = string.Empty;

        [Column("usuario")]
        [StringLength(100)]
        public string Usuario { get; set; } = string.Empty;

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Column("estado_contrato")]
        [StringLength(100)]
        public string? EstadoContrato { get; set; }

        // Relación con el contrato
        [ForeignKey("IdContrato")]
        public virtual Contrato? Contrato { get; set; }
    }
}
