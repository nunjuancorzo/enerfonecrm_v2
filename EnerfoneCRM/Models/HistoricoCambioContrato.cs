using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("historicocambioscontratos")]
public class HistoricoCambioContrato
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("idcontrato")]
    public int IdContrato { get; set; }

    [Column("campo")]
    [MaxLength(100)]
    public string Campo { get; set; } = string.Empty;

    [Column("dato")]
    [MaxLength(500)]
    public string? Dato { get; set; }

    [Column("usuario")]
    [MaxLength(100)]
    public string? Usuario { get; set; }

    [Column("fecha_cambio")]
    public DateTime FechaCambio { get; set; }
}
