using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    /// <summary>
    /// Detalle de distribución jerárquica de comisiones en una liquidación específica.
    /// Registra cuánto recibe cada nivel de la jerarquía (colaborador, gestor, jefe, director, administrador).
    /// </summary>
    [Table("detalle_comision_liquidacion")]
    public class DetalleComisionLiquidacion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// ID de la liquidación histórica a la que pertenece este detalle
        /// </summary>
        [Required]
        [Column("historico_liquidacion_id")]
        public int HistoricoLiquidacionId { get; set; }

        /// <summary>
        /// ID del contrato que genera esta comisión
        /// </summary>
        [Required]
        [Column("contrato_id")]
        public int ContratoId { get; set; }

        /// <summary>
        /// Tipo de contrato: "energia", "telefonia", "alarma"
        /// </summary>
        [MaxLength(20)]
        [Column("tipo_contrato")]
        public string? TipoContrato { get; set; }

        /// <summary>
        /// Comisión base del contrato (100% antes de distribución)
        /// </summary>
        [Column("comision_base", TypeName = "decimal(10,2)")]
        public decimal ComisionBase { get; set; }

        // ===== COMISIONES POR NIVEL =====

        /// <summary>
        /// ID del colaborador (usuario que registró el contrato)
        /// </summary>
        [Column("colaborador_id")]
        public int ColaboradorId { get; set; }

        /// <summary>
        /// Importe de comisión del colaborador
        /// </summary>
        [Column("comision_colaborador", TypeName = "decimal(10,2)")]
        public decimal ComisionColaborador { get; set; }

        /// <summary>
        /// Porcentaje aplicado al colaborador
        /// </summary>
        [Column("porcentaje_colaborador", TypeName = "decimal(5,2)")]
        public decimal PorcentajeColaborador { get; set; }

        /// <summary>
        /// ID del gestor (si existe en la jerarquía)
        /// </summary>
        [Column("gestor_id")]
        public int? GestorId { get; set; }

        /// <summary>
        /// Importe de comisión del gestor
        /// </summary>
        [Column("comision_gestor", TypeName = "decimal(10,2)")]
        public decimal? ComisionGestor { get; set; }

        /// <summary>
        /// Porcentaje aplicado al gestor
        /// </summary>
        [Column("porcentaje_gestor", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeGestor { get; set; }

        /// <summary>
        /// ID del jefe de ventas (si existe en la jerarquía)
        /// </summary>
        [Column("jefe_ventas_id")]
        public int? JefeVentasId { get; set; }

        /// <summary>
        /// Importe de comisión del jefe de ventas
        /// </summary>
        [Column("comision_jefe_ventas", TypeName = "decimal(10,2)")]
        public decimal? ComisionJefeVentas { get; set; }

        /// <summary>
        /// Porcentaje aplicado al jefe de ventas
        /// </summary>
        [Column("porcentaje_jefe_ventas", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeJefeVentas { get; set; }

        /// <summary>
        /// ID del director comercial (si existe en la jerarquía)
        /// </summary>
        [Column("director_comercial_id")]
        public int? DirectorComercialId { get; set; }

        /// <summary>
        /// Importe de comisión del director comercial
        /// </summary>
        [Column("comision_director_comercial", TypeName = "decimal(10,2)")]
        public decimal? ComisionDirectorComercial { get; set; }

        /// <summary>
        /// Porcentaje aplicado al director comercial
        /// </summary>
        [Column("porcentaje_director_comercial", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeDirectorComercial { get; set; }

        /// <summary>
        /// ID del administrador (siempre recibe el % restante)
        /// </summary>
        [Column("administrador_id")]
        public int AdministradorId { get; set; }

        /// <summary>
        /// Importe de comisión del administrador (100% - suma de otros)
        /// </summary>
        [Column("comision_administrador", TypeName = "decimal(10,2)")]
        public decimal ComisionAdministrador { get; set; }

        /// <summary>
        /// Porcentaje aplicado al administrador
        /// </summary>
        [Column("porcentaje_administrador", TypeName = "decimal(5,2)")]
        public decimal PorcentajeAdministrador { get; set; }

        /// <summary>
        /// Nombre del proveedor (Comercializadora/Operadora/EmpresaAlarma)
        /// </summary>
        [MaxLength(200)]
        [Column("nombre_proveedor")]
        public string? NombreProveedor { get; set; }

        /// <summary>
        /// Tipo de proveedor: "Comercializadora", "Operadora", "EmpresaAlarma"
        /// </summary>
        [MaxLength(50)]
        [Column("tipo_proveedor")]
        public string? TipoProveedor { get; set; }

        /// <summary>
        /// Fecha de creación del detalle
        /// </summary>
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("HistoricoLiquidacionId")]
        public HistoricoLiquidacion? HistoricoLiquidacion { get; set; }

        [ForeignKey("ContratoId")]
        public Contrato? Contrato { get; set; }

        [NotMapped]
        public decimal TotalDistribuido => 
            ComisionColaborador + 
            (ComisionGestor ?? 0) + 
            (ComisionJefeVentas ?? 0) + 
            (ComisionDirectorComercial ?? 0) + 
            ComisionAdministrador;
    }
}
