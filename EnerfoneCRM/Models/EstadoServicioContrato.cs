using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("contrato_estado_servicio")]
public class EstadoServicioContrato
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("contrato_id")]
    public int ContratoId { get; set; }

    [Required]
    [Column("clave_linea")]
    [StringLength(40)]
    public string ClaveLinea { get; set; } = string.Empty;

    [Column("estado")]
    [StringLength(40)]
    public string Estado { get; set; } = string.Empty;

    [Column("fecha_instalacion")]
    public DateTime? FechaInstalacion { get; set; }
}