using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("empresas")]
public class Empresa
{
    [Key]
    [Column("id_empresa")]
    public int Id { get; set; }

    [Required, StringLength(500)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("activa")]
    public bool Activa { get; set; } = true;

    [Column("formulario_optime_habilitado")]
    public bool FormularioOptimeHabilitado { get; set; } = false;
}