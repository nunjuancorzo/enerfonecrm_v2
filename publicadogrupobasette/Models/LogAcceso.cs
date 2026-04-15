using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("log_accesos")]
public class LogAcceso
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("nombre_usuario")]
    [StringLength(45)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Column("rol")]
    [StringLength(45)]
    public string Rol { get; set; } = string.Empty;

    [Column("fecha_acceso")]
    public DateTime FechaAcceso { get; set; } = DateTime.Now;
}
