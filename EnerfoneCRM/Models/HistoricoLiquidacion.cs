using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("historico_liquidaciones")]
public class HistoricoLiquidacion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("usuario_nombre")]
    [StringLength(100)]
    public string UsuarioNombre { get; set; } = string.Empty;

    [Column("usuario_email")]
    [StringLength(255)]
    public string? UsuarioEmail { get; set; }

    [Column("cantidad_contratos")]
    public int CantidadContratos { get; set; }

    [Column("contratos_energia")]
    public int ContratosEnergia { get; set; }

    [Column("contratos_telefonia")]
    public int ContratosTelefonia { get; set; }

    [Column("contratos_alarmas")]
    public int ContratosAlarmas { get; set; }

    [Column("fecha_aprobacion")]
    public DateTime FechaAprobacion { get; set; }

    [Column("aprobado_por_id")]
    public int AprobadoPorId { get; set; }

    [Column("aprobado_por_nombre")]
    [StringLength(100)]
    public string AprobadoPorNombre { get; set; } = string.Empty;

    [Column("observaciones")]
    [StringLength(500)]
    public string? Observaciones { get; set; }

    [NotMapped]
    public bool Reactivando { get; set; }
}
