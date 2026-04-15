using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("usuario_operadoras")]
public class UsuarioOperadora
{
    [Key]
    [Column("idusuario_operadoras")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("operadora_id")]
    public int OperadoraId { get; set; }

    [Column("fecha_asignacion")]
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
}
