using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("tarifastelefonia")]
    public class TarifaTelefonia
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("compania")]
        [StringLength(255)]
        public string Compania { get; set; } = string.Empty;

        [Required]
        [Column("tipo")]
        [StringLength(255)]
        public string Tipo { get; set; } = string.Empty;

        [Column("fibra")]
        [StringLength(255)]
        public string? Fibra { get; set; }

        [Column("gbmovil")]
        [StringLength(255)]
        public string? GbMovil { get; set; }

        [Column("precio")]
        [StringLength(255)]
        public string? Precio { get; set; }

        [Column("comision")]
        [StringLength(255)]
        public string? Comision { get; set; }

        [Required]
        [Column("precioNew")]
        public decimal PrecioNew { get; set; }

        [Required]
        [Column("comisionNew")]
        public decimal ComisionNew { get; set; }

        [Column("TV")]
        [StringLength(100)]
        public string? TV { get; set; }
    }
}
