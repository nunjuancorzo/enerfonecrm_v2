using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("log_activaciones_contratos")]
    public class LogActivacionContrato
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("contrato_id")]
        public int ContratoId { get; set; }

        [Column("fecha_activacion")]
        [Required]
        public DateTime FechaActivacion { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Column("usuario")]
        [MaxLength(255)]
        public string? Usuario { get; set; }

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        // Relación de navegación
        [ForeignKey("ContratoId")]
        public Contrato? Contrato { get; set; }
    }
}
