using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("tarifasgas")]
    public class TarifaGas
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("empresa")]
        [StringLength(255)]
        public string Empresa { get; set; } = string.Empty;

        [Required]
        [Column("tipo_cliente")]
        [StringLength(50)]
        public string TipoCliente { get; set; } = string.Empty;

        [Required]
        [Column("nombre")]
        [StringLength(255)]
        public string Nombre { get; set; } = string.Empty;

        [Column("peaje_gas")]
        [StringLength(50)]
        public string? PeajeGas { get; set; }

        [Column("termino_fijo_gas")]
        [StringLength(255)]
        public string? TerminoFijoGas { get; set; }

        [Column("termino_variable_gas")]
        [StringLength(255)]
        public string? TerminoVariableGas { get; set; }

        [Column("pvd_sva")]
        [StringLength(255)]
        public string? PvdSva { get; set; }

        [Column("descuento")]
        [StringLength(255)]
        public string? Descuento { get; set; }

        [Column("observaciones_descuentos")]
        [StringLength(500)]
        public string? ObservacionesDescuentos { get; set; }

        [Column("permanencia")]
        [StringLength(100)]
        public string? Permanencia { get; set; }

        [Column("dias_penalizacion")]
        public int? DiasPenalizacion { get; set; }

        [Column("tipo_penalizacion")]
        [StringLength(20)]
        public string? TipoPenalizacion { get; set; } // "Total" o "Proporcional"

        [Required]
        [Column("comision")]
        public decimal Comision { get; set; }

        [Required]
        [Column("precioNew")]
        public decimal PrecioNew { get; set; }

        [Required]
        [Column("activa")]
        public bool Activa { get; set; } = true;

        [Column("fecha_carga")]
        public DateTime? FechaCarga { get; set; }
    }
}
