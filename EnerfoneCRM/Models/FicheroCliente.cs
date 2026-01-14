using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("ficherosclientes")]
    public class FicheroCliente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("idCliente")]
        public int IdCliente { get; set; }

        [Column("tipoFichero")]
        [MaxLength(50)]
        public string TipoFichero { get; set; } = string.Empty; // DNIParticular, DNIPyme, CIFPyme, EscriturasPyme

        [Column("nombreArchivo")]
        [MaxLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Column("contenidoArchivo")]
        public byte[]? ContenidoArchivo { get; set; }

        [Column("fechaCarga")]
        public DateTime? FechaCarga { get; set; }

        // Navegación
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }
    }
}
