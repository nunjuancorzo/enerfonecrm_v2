using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("plantillas_precarga")]
public class PlantillaPreCarga
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Column("comercializadora")]
    [StringLength(255)]
    [Required]
    public string Comercializadora { get; set; } = string.Empty;

    [Column("alias_comercializadora")]
    [StringLength(500)]
    public string? AliasComercializadora { get; set; } // Separados por comas

    [Column("tipo_energia")]
    [StringLength(50)]
    [Required]
    public string TipoEnergia { get; set; } = string.Empty; // "LUZ", "GAS", "LUZ+GAS"

    [Column("variante_factura")]
    [StringLength(255)]
    public string? VarianteFactura { get; set; }

    [Column("prioridad")]
    public int Prioridad { get; set; } = 0;

    [Column("activa")]
    public bool Activa { get; set; } = true;

    [Column("campos_mapeados", TypeName = "TEXT")]
    public string? CamposMapeados { get; set; } // JSON con los campos mapeados

    [Column("notas_internas", TypeName = "TEXT")]
    public string? NotasInternas { get; set; }

    [Column("archivo_factura_ejemplo")]
    [StringLength(500)]
    public string? ArchivoFacturaEjemplo { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("fecha_modificacion")]
    public DateTime? FechaModificacion { get; set; }

    [Column("usuario_creador_id")]
    public int? UsuarioCreadorId { get; set; }

    [ForeignKey("UsuarioCreadorId")]
    public Usuario? UsuarioCreador { get; set; }
}
