using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("ficheroscontratos")]
    public class FicheroContrato
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("idContrato")]
        public int IdContrato { get; set; }

        [Required]
        [Column("tipoFichero")]
        [StringLength(100)]
        public string TipoFichero { get; set; } = string.Empty;

        [Required]
        [Column("fichero")]
        public byte[] Fichero { get; set; } = Array.Empty<byte>();

        [Column("nombreFichero")]
        [StringLength(255)]
        public string? NombreFichero { get; set; }

        // Propiedad de navegación
        [ForeignKey("IdContrato")]
        public Contrato? Contrato { get; set; }
    }
}
