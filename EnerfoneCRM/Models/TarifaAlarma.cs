using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("tarifas_alarmas")]
    public class TarifaAlarma
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tipo")]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty; // Kit, Opcional, Campaña

        [Required]
        [Column("tipo_inmueble")]
        [StringLength(50)]
        public string TipoInmueble { get; set; } = string.Empty; // Hogar, Negocio

        [Required]
        [Column("nombre_tarifa")]
        [StringLength(255)]
        public string NombreTarifa { get; set; } = string.Empty;

        [Required]
        [Column("cuota_mensual")]
        public decimal CuotaMensual { get; set; }

        [Required]
        [Column("permanencia")]
        public int Permanencia { get; set; } // 12, 24, 36 meses

        [Column("empresa")]
        [StringLength(255)]
        public string? Empresa { get; set; }

        [Column("comision")]
        public decimal? Comision { get; set; }

        [Column("descripcion")]
        [StringLength(500)]
        public string? Descripcion { get; set; }

        // Campos de penalización por baja anticipada (marzo 2026)
        [Column("dias_penalizacion")]
        public int? DiasPenalizacion { get; set; }

        [Column("tipo_penalizacion")]
        [StringLength(20)]
        public string? TipoPenalizacion { get; set; } // "Total" o "Proporcional"

        [Column("activa")]
        public bool Activa { get; set; } = true;
    }
}
