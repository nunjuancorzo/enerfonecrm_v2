using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("operadoras")]
    public class Operadora
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Column("logo_archivo")]
        [StringLength(255)]
        public string? LogoArchivo { get; set; }

        [Column("logo_contenido")]
        public byte[]? LogoContenido { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("requiere_icc")]
        public bool RequiereICC { get; set; } = false;

        [Column("fecha_creacion")]
        public DateTime? FechaCreacion { get; set; }
    }
}
