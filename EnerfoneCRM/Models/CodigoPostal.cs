using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("codigos_postales")]
    public class CodigoPostal
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("codigo_postal")]
        [Required(ErrorMessage = "El código postal es obligatorio")]
        [MaxLength(5)]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 dígitos")]
        public string CodigoPostalValor { get; set; } = string.Empty;

        [Column("ciudad")]
        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [MaxLength(100)]
        public string Ciudad { get; set; } = string.Empty;

        [Column("provincia")]
        [Required(ErrorMessage = "La provincia es obligatoria")]
        [MaxLength(50)]
        public string Provincia { get; set; } = string.Empty;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_creacion")]
        public DateTime? FechaCreacion { get; set; }

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }
    }
}
