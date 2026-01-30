using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("usuario_empresas_alarmas")]
public class UsuarioEmpresaAlarma
{
    [Key]
    [Column("idusuario_empresa_alarma")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("empresa_alarma_id")]
    public int EmpresaAlarmaId { get; set; }

    [Column("fecha_asignacion")]
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
}
