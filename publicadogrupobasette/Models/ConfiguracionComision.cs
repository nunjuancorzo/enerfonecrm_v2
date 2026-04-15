using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    /// <summary>
    /// Configuración de comisiones específicas por usuario y proveedor (Comercializadora/Operadora/EmpresaAlarma).
    /// Permite definir porcentajes de comisión diferenciados por usuario y por jerarquía.
    /// </summary>
    [Table("configuracion_comisiones")]
    public class ConfiguracionComision
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario al que se aplica esta configuración
        /// </summary>
        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Tipo de proveedor: "Comercializadora", "Operadora", "EmpresaAlarma"
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("tipo_proveedor")]
        public string TipoProveedor { get; set; } = string.Empty;

        /// <summary>
        /// ID del proveedor específico (ComercializadoraId, OperadoraId o EmpresaAlarmaId)
        /// </summary>
        [Required]
        [Column("proveedor_id")]
        public int ProveedorId { get; set; }

        /// <summary>
        /// Nombre del proveedor (para referencia rápida)
        /// </summary>
        [MaxLength(200)]
        [Column("nombre_proveedor")]
        public string? NombreProveedor { get; set; }

        /// <summary>
        /// Porcentaje de comisión para el colaborador (0-100)
        /// Por defecto, este usuario recibe este % de la comisión base
        /// </summary>
        [Column("porcentaje_colaborador", TypeName = "decimal(5,2)")]
        public decimal PorcentajeColaborador { get; set; }

        /// <summary>
        /// Porcentaje de comisión para el gestor (0-100)
        /// NULL si no aplica distribución a gestor
        /// </summary>
        [Column("porcentaje_gestor", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeGestor { get; set; }

        /// <summary>
        /// Porcentaje de comisión para el jefe de ventas (0-100)
        /// NULL si no aplica distribución a jefe
        /// </summary>
        [Column("porcentaje_jefe_ventas", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeJefeVentas { get; set; }

        /// <summary>
        /// Porcentaje de comisión para el director comercial (0-100)
        /// NULL si no aplica distribución a director
        /// </summary>
        [Column("porcentaje_director_comercial", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeDirectorComercial { get; set; }

        /// <summary>
        /// El porcentaje restante (100% - suma de todos) se asigna automáticamente al Administrador
        /// </summary>
        [NotMapped]
        public decimal PorcentajeAdministrador
        {
            get
            {
                decimal total = PorcentajeColaborador
                    + (PorcentajeGestor ?? 0)
                    + (PorcentajeJefeVentas ?? 0)
                    + (PorcentajeDirectorComercial ?? 0);

                return Math.Max(0, 100 - total);
            }
        }

        /// <summary>
        /// Fecha de creación de esta configuración
        /// </summary>
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Indica si esta configuración está activa
        /// </summary>
        [Column("activa")]
        public bool Activa { get; set; } = true;

        // Navegación
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
