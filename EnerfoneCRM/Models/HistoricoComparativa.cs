using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("historico_comparativas")]
public class HistoricoComparativa
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("fecha_comparativa")]
    public DateTime FechaComparativa { get; set; }

    [Column("origen")]
    [StringLength(50)]
    public string Origen { get; set; } = string.Empty; // "frontend", "backend"

    [Column("email_cliente")]
    [StringLength(255)]
    public string? EmailCliente { get; set; }

    [Column("tipo_energia")]
    [StringLength(50)]
    public string TipoEnergia { get; set; } = string.Empty; // "LUZ", "GAS", "LUZ+GAS"

    [Column("cups")]
    [StringLength(255)]
    public string? Cups { get; set; }

    [Column("comercializadora_actual")]
    [StringLength(255)]
    public string? ComercializadoraActual { get; set; }

    [Column("tarifa_actual")]
    [StringLength(255)]
    public string? TarifaActual { get; set; }

    [Column("total_factura_actual", TypeName = "decimal(10,2)")]
    public decimal? TotalFacturaActual { get; set; }

    [Column("mejor_tarifa_id")]
    public int? MejorTarifaId { get; set; }

    [Column("mejor_tarifa_nombre")]
    [StringLength(255)]
    public string? MejorTarifaNombre { get; set; }

    [Column("mejor_tarifa_empresa")]
    [StringLength(255)]
    public string? MejorTarifaEmpresa { get; set; }

    [Column("ahorro_mensual", TypeName = "decimal(10,2)")]
    public decimal? AhorroMensual { get; set; }

    [Column("ahorro_anual", TypeName = "decimal(10,2)")]
    public decimal? AhorroAnual { get; set; }

    [Column("porcentaje_ahorro", TypeName = "decimal(5,2)")]
    public decimal? PorcentajeAhorro { get; set; }

    [Column("datos_utilizados", TypeName = "TEXT")]
    public string? DatosUtilizados { get; set; } // JSON con los datos usados en el cálculo

    [Column("resultado_ranking", TypeName = "TEXT")]
    public string? ResultadoRanking { get; set; } // JSON con el ranking completo

    [Column("proveedor_ocr")]
    [StringLength(100)]
    public string? ProveedorOcr { get; set; }

    [Column("email_enviado")]
    public bool EmailEnviado { get; set; }

    [Column("fecha_envio_email")]
    public DateTime? FechaEnvioEmail { get; set; }

    [Column("advertencias", TypeName = "TEXT")]
    public string? Advertencias { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    public Usuario? Usuario { get; set; }

    [Column("nombre_archivo_factura")]
    [StringLength(500)]
    public string? NombreArchivoFactura { get; set; }
}
