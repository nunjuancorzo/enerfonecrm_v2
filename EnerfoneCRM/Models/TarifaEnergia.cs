using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("tarifasenergia")]
    public class TarifaEnergia
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("empresa")]
        [StringLength(255)]
        public string Empresa { get; set; } = string.Empty;

        [Required]
        [Column("tipo")]
        [StringLength(255)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [Column("nombre")]
        [StringLength(255)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("potencia1")]
        [StringLength(255)]
        public string Potencia1 { get; set; } = string.Empty;

        [Required]
        [Column("energia1")]
        [StringLength(255)]
        public string Energia1 { get; set; } = string.Empty;

        [Column("precio")]
        [StringLength(255)]
        public string? Precio { get; set; }

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

        [Required]
        [Column("comision")]
        public decimal Comision { get; set; }

        [Required]
        [Column("precioNew")]
        public decimal PrecioNew { get; set; }

        // Campos para comparativa de tarifas (añadidos 7 enero 2026)
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
    }
}
