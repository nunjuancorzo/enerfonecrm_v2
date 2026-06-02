using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("usuario_comision_proveedores")]
public class UsuarioComisionProveedor
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("tipo_proveedor", TypeName = "varchar(50)")]
    public string TipoProveedor { get; set; } = string.Empty; // "operadora", "comercializadora", "empresa_alarma"

    [Column("proveedor_id")]
    public int ProveedorId { get; set; }

    [Column("porcentaje_comision", TypeName = "decimal(5,2)")]
    public decimal PorcentajeComision { get; set; } = 0m;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("fecha_actualizacion")]
    public DateTime FechaActualizacion { get; set; } = DateTime.Now;

    // Relación con Usuario
    [ForeignKey("UsuarioId")]
    public virtual Usuario? Usuario { get; set; }
}
