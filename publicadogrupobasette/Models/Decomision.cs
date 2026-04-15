using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    /// <summary>
    /// Registra decomisiones (penalizaciones) por bajas anticipadas de contratos.
    /// Se crean cuando un contrato se cancela antes de cumplir el periodo de penalización.
    /// </summary>
    [Table("decomisiones")]
    public class Decomision
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// ID del contrato que genera la decomisión
        /// </summary>
        [Required]
        [Column("contrato_id")]
        public int ContratoId { get; set; }

        /// <summary>
        /// ID del usuario que originalmente recibió la comisión
        /// </summary>
        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Nombre del usuario (para referencia)
        /// </summary>
        [MaxLength(200)]
        [Column("nombre_usuario")]
        public string? NombreUsuario { get; set; }

        /// <summary>
        /// ID de la liquidación original donde se pagó la comisión
        /// </summary>
        [Column("liquidacion_original_id")]
        public int? LiquidacionOriginalId { get; set; }

        /// <summary>
        /// ID de la liquidación donde se registra esta decomisión (como importe negativo)
        /// </summary>
        [Column("liquidacion_decomision_id")]
        public int? LiquidacionDecomisionId { get; set; }

        /// <summary>
        /// Tipo de decomisión: "Total" o "Proporcional"
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("tipo_decomision")]
        public string TipoDecomision { get; set; } = "Total";

        /// <summary>
        /// Comisión original que se pagó
        /// </summary>
        [Column("comision_original", TypeName = "decimal(10,2)")]
        public decimal ComisionOriginal { get; set; }

        /// <summary>
        /// Importe de la decomisión (siempre positivo, se registra como negativo en liquidación)
        /// </summary>
        [Column("importe_decomision", TypeName = "decimal(10,2)")]
        public decimal ImporteDecomision { get; set; }

        /// <summary>
        /// Periodo de penalización en días (configurado en la tarifa)
        /// </summary>
        [Column("dias_penalizacion")]
        public int Diaspenalizacion { get; set; }

        /// <summary>
        /// Días que el contrato estuvo activo antes de la baja
        /// </summary>
        [Column("dias_activo")]
        public int DiasActivo { get; set; }

        /// <summary>
        /// Días pendientes de cumplir (DiasPenalizacion - DiasActivo)
        /// </summary>
        [Column("dias_pendientes")]
        public int DiasPendientes { get; set; }

        /// <summary>
        /// Fecha de alta/activación del contrato
        /// </summary>
        [Column("fecha_alta")]
        public DateTime? FechaAlta { get; set; }

        /// <summary>
        /// Fecha de baja del contrato
        /// </summary>
        [Column("fecha_baja")]
        public DateTime FechaBaja { get; set; }

        /// <summary>
        /// Fecha en que se registró esta decomisión
        /// </summary>
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Usuario que registró la baja y creó la decomisión
        /// </summary>
        [Column("creado_por_usuario_id")]
        public int? CreadoPorUsuarioId { get; set; }

        /// <summary>
        /// Observaciones sobre la decomisión
        /// </summary>
        [MaxLength(1000)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Estado: "Pendiente", "Aplicada", "Cancelada"
        /// </summary>
        [MaxLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Tipo de contrato: "energia", "telefonia", "alarma"
        /// </summary>
        [MaxLength(20)]
        [Column("tipo_contrato")]
        public string? TipoContrato { get; set; }

        /// <summary>
        /// Nombre del proveedor (para referencia)
        /// </summary>
        [MaxLength(200)]
        [Column("nombre_proveedor")]
        public string? NombreProveedor { get; set; }

        // Navegación
        [ForeignKey("ContratoId")]
        public Contrato? Contrato { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [ForeignKey("LiquidacionOriginalId")]
        public HistoricoLiquidacion? LiquidacionOriginal { get; set; }

        [ForeignKey("LiquidacionDecomisionId")]
        public HistoricoLiquidacion? LiquidacionDecomision { get; set; }

        /// <summary>
        /// Calcula el porcentaje de penalización aplicado
        /// </summary>
        [NotMapped]
        public decimal PorcentajePenalizacion
        {
            get
            {
                if (ComisionOriginal == 0) return 0;
                return Math.Round((ImporteDecomision / ComisionOriginal) * 100, 2);
            }
        }
    }
}
