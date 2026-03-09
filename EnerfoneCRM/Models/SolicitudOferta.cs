using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("solicitudes_ofertas")]
public class SolicitudOferta
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("nombre_comercial")]
    [StringLength(100)]
    public string NombreComercial { get; set; } = string.Empty;

    [Column("email_comercial")]
    [StringLength(100)]
    public string EmailComercial { get; set; } = string.Empty;

    // Datos del interesado
    [Column("nombre_interesado")]
    [StringLength(200)]
    public string? NombreInteresado { get; set; }

    [Column("telefono_interesado")]
    [StringLength(20)]
    public string? TelefonoInteresado { get; set; }

    [Column("email_interesado")]
    [StringLength(100)]
    public string? EmailInteresado { get; set; }

    // Tipos de oferta seleccionados (pueden ser múltiples)
    [Column("tipo_luz")]
    public bool TipoLuz { get; set; }

    [Column("tipo_gas")]
    public bool TipoGas { get; set; }

    [Column("tipo_fotovoltaica")]
    public bool TipoFotovoltaica { get; set; }

    [Column("tipo_fibra")]
    public bool TipoFibra { get; set; }

    [Column("tipo_movil")]
    public bool TipoMovil { get; set; }

    [Column("tipo_fibra_movil")]
    public bool TipoFibraMovil { get; set; }

    [Column("tipo_fibra_movil_tv")]
    public bool TipoFibraMovilTv { get; set; }

    [Column("tipo_alarma")]
    public bool TipoAlarma { get; set; }

    // Campos comunes para Luz y Gas
    [Column("luz_gas_tipo_cliente")]
    [StringLength(50)]
    public string? LuzGasTipoCliente { get; set; } // "Residencial" o "PYME"

    [Column("luz_gas_ruta_factura")]
    [StringLength(500)]
    public string? LuzGasRutaFactura { get; set; }

    [Column("luz_gas_observaciones")]
    [StringLength(2000)]
    public string? LuzGasObservaciones { get; set; }

    // Campos para Fotovoltaica
    [Column("fotovoltaica_ruta_factura")]
    [StringLength(500)]
    public string? FotovoltaicaRutaFactura { get; set; }

    [Column("fotovoltaica_enlace_maps")]
    [StringLength(500)]
    public string? FotovoltaicaEnlaceMaps { get; set; }

    [Column("fotovoltaica_observaciones")]
    [StringLength(2000)]
    public string? FotovoltaicaObservaciones { get; set; }

    // Campos para Telefonía (Fibra, Móvil, combinaciones)
    [Column("telefonia_tipo_solicitud")]
    [StringLength(50)]
    public string? TelefoniaTipoSolicitud { get; set; } // "Alta Nueva" o "Portabilidad"

    [Column("telefonia_ruta_factura")]
    [StringLength(500)]
    public string? TelefoniaRutaFactura { get; set; }

    [Column("telefonia_contrato_actual")]
    [StringLength(1000)]
    public string? TelefoniaContratoActual { get; set; }

    [Column("telefonia_observaciones")]
    [StringLength(2000)]
    public string? TelefoniaObservaciones { get; set; }

    // Campos para Alarma
    [Column("alarma_tipo")]
    [StringLength(50)]
    public string? AlarmaTipo { get; set; } // "Negocio" o "Residencial"

    [Column("alarma_tiene_actual")]
    public bool? AlarmaTieneActual { get; set; }

    [Column("alarma_observaciones")]
    [StringLength(2000)]
    public string? AlarmaObservaciones { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("estado")]
    [StringLength(50)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En Proceso, Completada

    [Column("observaciones_admin")]
    [StringLength(2000)]
    public string? ObservacionesAdmin { get; set; }

    [Column("fecha_procesado")]
    public DateTime? FechaProcesado { get; set; }
}
