using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("empresas_alarmas")]
    public class EmpresaAlarma
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        [Column("logo_archivo")]
        [MaxLength(255)]
        public string? LogoArchivo { get; set; }

        [Column("logo_contenido")]
        public byte[]? LogoContenido { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_creacion")]
        public DateTime? FechaCreacion { get; set; }
    }
}
