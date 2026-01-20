using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("contratos")]
    public class Contrato
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("tipo")]
        [MaxLength(50)]
        public string? Tipo { get; set; }

        [Column("estado")]
        [MaxLength(100)]
        public string? Estado { get; set; }

        [Column("comercial")]
        [MaxLength(255)]
        public string? Comercial { get; set; }

        [Column("fecha_creacion")]
        public DateTime? FechaCreacion { get; set; }

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }

        [Column("fecha_activo")]
        public DateTime? FechaActivo { get; set; }

        [Column("idCliente")]
        public int? IdCliente { get; set; }

        [Column("nombre_cliente")]
        [MaxLength(255)]
        public string? NombreCliente { get; set; }

        [Column("dni")]
        [MaxLength(50)]
        public string? Dni { get; set; }

        [Column("direccion")]
        [MaxLength(500)]
        public string? Direccion { get; set; }

        [Column("iban")]
        [MaxLength(100)]
        public string? Iban { get; set; }

        [Column("comision")]
        public decimal? Comision { get; set; }

        [Column("usuario_comercializadora_id")]
        public int? UsuarioComercializadoraId { get; set; }

        [Column("servicio_id")]
        public int? ServicioId { get; set; }

        // Campos específicos de ENERGÍA
        [Column("estadoServicio")]
        [MaxLength(100)]
        public string? EstadoServicio { get; set; }

        [Column("en_Comercializadora")]
        [MaxLength(100)]
        [Required(ErrorMessage = "La comercializadora es obligatoria")]
        public string? EnComercializadora { get; set; }

        [Column("en_Tarifa")]
        [MaxLength(255)]
        [Required(ErrorMessage = "La tarifa es obligatoria")]
        public string? EnTarifa { get; set; }

        [Column("en_CUPS")]
        [MaxLength(255)]
        public string? EnCups { get; set; }

        [Column("en_CUPSGas")]
        [MaxLength(255)]
        public string? EnCupsGas { get; set; }

        [Column("en_Servicios")]
        [MaxLength(255)]
        public string? EnServicios { get; set; }

        [Column("en_IBAN")]
        [MaxLength(100)]
        public string? EnIban { get; set; }

        [Column("tipoOperacion")]
        [MaxLength(100)]
        [Required(ErrorMessage = "El tipo de operación es obligatorio")]
        public string? TipoOperacion { get; set; }

        [Column("potencia_contratada_p1")]
        public decimal? PotenciaContratada { get; set; }

        [Column("consumo_ultimos_12_meses")]
        public decimal? ConsumoAnual { get; set; }

        // Campos específicos de TELEFONÍA
        [Column("operadora_tel")]
        [MaxLength(100)]
        [Required(ErrorMessage = "La operadora es obligatoria")]
        public string? OperadoraTel { get; set; }

        [Column("Tarifa_tel")]
        [MaxLength(255)]
        [Required(ErrorMessage = "La tarifa principal es obligatoria")]
        public string? TarifaTel { get; set; }

        [Column("TipoTarifa_tel")]
        [MaxLength(100)]
        [Required(ErrorMessage = "El tipo de tarifa es obligatorio")]
        public string? TipoTarifaTel { get; set; }

        [Column("fijo_tel")]
        [MaxLength(50)]
        public string? FijoTel { get; set; }

        [Column("LineaMovilPrincipal")]
        [MaxLength(50)]
        public string? LineaMovilPrincipal { get; set; }

        [Column("tipo_linea_movil_principal")]
        [MaxLength(20)]
        public string? TipoLineaMovilPrincipal { get; set; } // "Contrato" o "Prepago"

        [Column("codigo_icc_principal")]
        [MaxLength(19)]
        public string? CodigoIccPrincipal { get; set; }

        [Column("numero_lineas_tel")]
        public int? NumeroLineasTel { get; set; }

        [Column("telefono_linea1_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea1Tel { get; set; }

        [Column("tarifa_linea1_tel")]
        [MaxLength(255)]
        public string? TarifaLinea1Tel { get; set; }

        [Column("tipo_linea1_tel")]
        [MaxLength(20)]
        public string? TipoLinea1Tel { get; set; }

        [Column("codigo_icc_linea1_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea1Tel { get; set; }

        [Column("telefono_linea2_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea2Tel { get; set; }

        [Column("tarifa_linea2_tel")]
        [MaxLength(255)]
        public string? TarifaLinea2Tel { get; set; }

        [Column("tipo_linea2_tel")]
        [MaxLength(20)]
        public string? TipoLinea2Tel { get; set; }

        [Column("codigo_icc_linea2_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea2Tel { get; set; }

        [Column("telefono_linea3_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea3Tel { get; set; }

        [Column("tarifa_linea3_tel")]
        [MaxLength(255)]
        public string? TarifaLinea3Tel { get; set; }

        [Column("tipo_linea3_tel")]
        [MaxLength(20)]
        public string? TipoLinea3Tel { get; set; }

        [Column("codigo_icc_linea3_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea3Tel { get; set; }

        [Column("telefono_linea4_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea4Tel { get; set; }

        [Column("tarifa_linea4_tel")]
        [MaxLength(255)]
        public string? TarifaLinea4Tel { get; set; }

        [Column("tipo_linea4_tel")]
        [MaxLength(20)]
        public string? TipoLinea4Tel { get; set; }

        [Column("codigo_icc_linea4_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea4Tel { get; set; }

        [Column("telefono_linea5_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea5Tel { get; set; }

        [Column("tarifa_linea5_tel")]
        [MaxLength(255)]
        public string? TarifaLinea5Tel { get; set; }

        [Column("tipo_linea5_tel")]
        [MaxLength(20)]
        public string? TipoLinea5Tel { get; set; }

        [Column("codigo_icc_linea5_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea5Tel { get; set; }

        [Column("fecha_instalacion_tel")]
        public DateTime? FechaInstalacionTel { get; set; }

        [Column("horario_instalacion_tel")]
        [MaxLength(100)]
        public string? HorarioInstalacionTel { get; set; }

        [Column("contratar")]
        [MaxLength(255)]
        public string? Contratar { get; set; }

        [NotMapped]
        public string? CuentaBancaria { get; set; }

        [Column("TV")]
        [MaxLength(50)]
        public string? Tv { get; set; }

        // Campos específicos de ALARMAS
        [Column("tipo_alarma")]
        [MaxLength(50)]
        [Required(ErrorMessage = "El tipo de inmueble es obligatorio")]
        public string? TipoAlarma { get; set; } // Hogar, Negocio

        [Column("subtipo_inmueble")]
        [MaxLength(100)]
        [Required(ErrorMessage = "El subtipo es obligatorio")]
        public string? SubtipoInmueble { get; set; } // Piso, Bajo, Chalet, etc.

        [Column("tiene_contrato_anterior")]
        public bool? TieneContratoAnterior { get; set; }

        [Column("compania_anterior")]
        [MaxLength(255)]
        public string? CompaniaAnterior { get; set; }

        [Column("numero_contrato_anterior")]
        [MaxLength(100)]
        public string? NumeroContratoAnterior { get; set; }

        [Column("fecha_permanencia_anterior")]
        public DateTime? FechaPermanenciaAnterior { get; set; }

        [Column("kit_alarma")]
        [MaxLength(255)]
        [Required(ErrorMessage = "El kit es obligatorio")]
        public string? KitAlarma { get; set; }

        [Column("opcionales_alarma")]
        [MaxLength(500)]
        public string? OpcionalesAlarma { get; set; }

        [Column("campana_alarma")]
        [MaxLength(255)]
        [Required(ErrorMessage = "La campaña es obligatoria")]
        public string? CampanaAlarma { get; set; }

        [Column("empresa_alarma")]
        [MaxLength(255)]
        [Required(ErrorMessage = "La empresa es obligatoria")]
        public string? EmpresaAlarma { get; set; }

        [Column("direccion_instalacion_alarma")]
        [MaxLength(500)]
        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string? DireccionInstalacionAlarma { get; set; }

        [Column("numero_instalacion")]
        [MaxLength(20)]
        [Required(ErrorMessage = "El número es obligatorio")]
        public string? NumeroInstalacion { get; set; }

        [Column("escalera_instalacion")]
        [MaxLength(10)]
        public string? EscaleraInstalacion { get; set; }

        [Column("piso_instalacion")]
        [MaxLength(10)]
        public string? PisoInstalacion { get; set; }

        [Column("puerta_instalacion")]
        [MaxLength(10)]
        public string? PuertaInstalacion { get; set; }

        [Column("codigo_postal_instalacion")]
        [MaxLength(10)]
        [Required(ErrorMessage = "El código postal es obligatorio")]
        public string? CodigoPostalInstalacion { get; set; }

        [Column("provincia_instalacion")]
        [MaxLength(100)]
        [Required(ErrorMessage = "La provincia es obligatoria")]
        public string? ProvinciaInstalacion { get; set; }

        [Column("localidad_instalacion")]
        [MaxLength(100)]
        [Required(ErrorMessage = "La localidad es obligatoria")]
        public string? LocalidadInstalacion { get; set; }

        [Column("aclarador_instalacion")]
        [MaxLength(500)]
        public string? AclaradorInstalacion { get; set; }

        [Column("observaciones_alarma")]
        public string? ObservacionesAlarma { get; set; }

        [Column("observaciones_estado")]
        public string? ObservacionesEstado { get; set; }

        [NotMapped] // Campo LONGBLOB en BD
        public byte[]? EnFactura { get; set; }

        [NotMapped] // Campo BLOB en BD
        public byte[]? Factura { get; set; }
        [Column("titular_iban_diferente")]
        public bool TitularIbanDiferente { get; set; }

        [Column("titular_iban_dni")]
        [MaxLength(50)]
        public string? TitularIbanDni { get; set; }

        [Column("titular_iban_nombre")]
        [MaxLength(255)]
        public string? TitularIbanNombre { get; set; }

        [Column("titular_iban_numero")]
        [MaxLength(100)]
        public string? TitularIbanNumero { get; set; }

        [NotMapped] // Campo LONGBLOB en BD
        public byte[]? FacturaPdfTel { get; set; }

        // PDF del contrato
        [Column("pdf_contrato_url")]
        [MaxLength(500)]
        public string? PdfContratoUrl { get; set; }

        // Navegación
        [NotMapped]
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }
    }
}
