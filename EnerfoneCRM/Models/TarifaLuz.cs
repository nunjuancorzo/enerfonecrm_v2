using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("tarifasluz")]
    public class TarifaLuz
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

        [Column("peaje")]
        [StringLength(50)]
        public string? Peaje { get; set; }

        [Column("potencia1")]
        [StringLength(255)]
        public string? Potencia1 { get; set; }

        [Column("potencia2")]
        [StringLength(255)]
        public string? Potencia2 { get; set; }

        [Column("potencia3")]
        [StringLength(255)]
        public string? Potencia3 { get; set; }

        [Column("potencia4")]
        [StringLength(255)]
        public string? Potencia4 { get; set; }

        [Column("potencia5")]
        [StringLength(255)]
        public string? Potencia5 { get; set; }

        [Column("potencia6")]
        [StringLength(255)]
        public string? Potencia6 { get; set; }

        [Column("energia1")]
        [StringLength(255)]
        public string? Energia1 { get; set; }

        [Column("energia2")]
        [StringLength(255)]
        public string? Energia2 { get; set; }

        [Column("energia3")]
        [StringLength(255)]
        public string? Energia3 { get; set; }

        [Column("energia4")]
        [StringLength(255)]
        public string? Energia4 { get; set; }

        [Column("energia5")]
        [StringLength(255)]
        public string? Energia5 { get; set; }

        [Column("energia6")]
        [StringLength(255)]
        public string? Energia6 { get; set; }

        [Column("termino_fijo_diario")]
        public decimal? TerminoFijoDiario { get; set; }

        [Column("precio_potencia_p1")]
        public decimal? PrecioPotenciaP1 { get; set; }

        [Column("precio_potencia_p2")]
        public decimal? PrecioPotenciaP2 { get; set; }

        [Column("precio_potencia_p3")]
        public decimal? PrecioPotenciaP3 { get; set; }

        [Column("precio_energia_p1")]
        public decimal? PrecioEnergiaP1 { get; set; }

        [Column("precio_energia_p2")]
        public decimal? PrecioEnergiaP2 { get; set; }

        [Column("precio_energia_p3")]
        public decimal? PrecioEnergiaP3 { get; set; }

        [Column("descuento")]
        [StringLength(255)]
        public string? Descuento { get; set; }

        [Column("observaciones_descuentos")]
        [StringLength(500)]
        public string? ObservacionesDescuentos { get; set; }

        [Column("excedentes")]
        [StringLength(100)]
        public string? Excedentes { get; set; }

        [Column("bateria_virtual")]
        [StringLength(50)]
        public string? BateriaVirtual { get; set; }

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
