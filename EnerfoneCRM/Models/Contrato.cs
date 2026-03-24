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

        [Column("id_contrato_externo")]
        [MaxLength(100)]
        public string? IdContratoExterno { get; set; }

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

        [Column("fecha_alta")]
        public DateTime? FechaAlta { get; set; }

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

        [Column("historico_liquidacion_id")]
        public int? HistoricoLiquidacionId { get; set; }

        // Campos específicos de ENERGÍA
        [Column("estadoServicio")]
        [MaxLength(100)]
        public string? EstadoServicio { get; set; }

        [Column("en_Comercializadora")]
        [MaxLength(100)]
        public string? EnComercializadora { get; set; }

        [Column("en_Tarifa")]
        [MaxLength(255)]
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
        public string? TipoOperacion { get; set; }

        [Column("potencia_contratada_p1")]
        public decimal? PotenciaContratada { get; set; }

        [Column("consumo_ultimos_12_meses")]
        public decimal? ConsumoAnual { get; set; }

        [Column("consumo_anual_gas")]
        public decimal? ConsumoAnualGas { get; set; }

        [Column("peaje_luz")]
        [MaxLength(50)]
        public string? PeajeLuz { get; set; }

        [Column("peaje_gas")]
        [MaxLength(50)]
        public string? PeajeGas { get; set; }

        // Campos específicos de TELEFONÍA
        [Column("operadora_tel")]
        [MaxLength(100)]
        public string? OperadoraTel { get; set; }

        [Column("Tarifa_tel")]
        [MaxLength(255)]
        public string? TarifaTel { get; set; }

        [Column("TipoTarifa_tel")]
        [MaxLength(100)]
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

        [Column("linea_movil_principal_2")]
        [MaxLength(50)]
        public string? LineaMovilPrincipal2 { get; set; }

        [Column("tipo_linea_movil_principal_2")]
        [MaxLength(20)]
        public string? TipoLineaMovilPrincipal2 { get; set; }

        [Column("codigo_icc_principal_2")]
        [MaxLength(19)]
        public string? CodigoIccPrincipal2 { get; set; }

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

        [Column("telefono_linea6_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea6Tel { get; set; }

        [Column("tarifa_linea6_tel")]
        [MaxLength(255)]
        public string? TarifaLinea6Tel { get; set; }

        [Column("tipo_linea6_tel")]
        [MaxLength(20)]
        public string? TipoLinea6Tel { get; set; }

        [Column("codigo_icc_linea6_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea6Tel { get; set; }

        [Column("telefono_linea7_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea7Tel { get; set; }

        [Column("tarifa_linea7_tel")]
        [MaxLength(255)]
        public string? TarifaLinea7Tel { get; set; }

        [Column("tipo_linea7_tel")]
        [MaxLength(20)]
        public string? TipoLinea7Tel { get; set; }

        [Column("codigo_icc_linea7_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea7Tel { get; set; }

        [Column("telefono_linea8_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea8Tel { get; set; }

        [Column("tarifa_linea8_tel")]
        [MaxLength(255)]
        public string? TarifaLinea8Tel { get; set; }

        [Column("tipo_linea8_tel")]
        [MaxLength(20)]
        public string? TipoLinea8Tel { get; set; }

        [Column("codigo_icc_linea8_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea8Tel { get; set; }

        [Column("telefono_linea9_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea9Tel { get; set; }

        [Column("tarifa_linea9_tel")]
        [MaxLength(255)]
        public string? TarifaLinea9Tel { get; set; }

        [Column("tipo_linea9_tel")]
        [MaxLength(20)]
        public string? TipoLinea9Tel { get; set; }

        [Column("codigo_icc_linea9_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea9Tel { get; set; }

        [Column("telefono_linea10_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea10Tel { get; set; }

        [Column("tarifa_linea10_tel")]
        [MaxLength(255)]
        public string? TarifaLinea10Tel { get; set; }

        [Column("tipo_linea10_tel")]
        [MaxLength(20)]
        public string? TipoLinea10Tel { get; set; }

        [Column("codigo_icc_linea10_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea10Tel { get; set; }

        [Column("telefono_linea11_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea11Tel { get; set; }

        [Column("tarifa_linea11_tel")]
        [MaxLength(255)]
        public string? TarifaLinea11Tel { get; set; }

        [Column("tipo_linea11_tel")]
        [MaxLength(20)]
        public string? TipoLinea11Tel { get; set; }

        [Column("codigo_icc_linea11_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea11Tel { get; set; }

        [Column("telefono_linea12_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea12Tel { get; set; }

        [Column("tarifa_linea12_tel")]
        [MaxLength(255)]
        public string? TarifaLinea12Tel { get; set; }

        [Column("tipo_linea12_tel")]
        [MaxLength(20)]
        public string? TipoLinea12Tel { get; set; }

        [Column("codigo_icc_linea12_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea12Tel { get; set; }

        [Column("telefono_linea13_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea13Tel { get; set; }

        [Column("tarifa_linea13_tel")]
        [MaxLength(255)]
        public string? TarifaLinea13Tel { get; set; }

        [Column("tipo_linea13_tel")]
        [MaxLength(20)]
        public string? TipoLinea13Tel { get; set; }

        [Column("codigo_icc_linea13_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea13Tel { get; set; }

        [Column("telefono_linea14_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea14Tel { get; set; }

        [Column("tarifa_linea14_tel")]
        [MaxLength(255)]
        public string? TarifaLinea14Tel { get; set; }

        [Column("tipo_linea14_tel")]
        [MaxLength(20)]
        public string? TipoLinea14Tel { get; set; }

        [Column("codigo_icc_linea14_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea14Tel { get; set; }

        [Column("telefono_linea15_tel")]
        [MaxLength(50)]
        public string? TelefonoLinea15Tel { get; set; }

        [Column("tarifa_linea15_tel")]
        [MaxLength(255)]
        public string? TarifaLinea15Tel { get; set; }

        [Column("tipo_linea15_tel")]
        [MaxLength(20)]
        public string? TipoLinea15Tel { get; set; }

        [Column("codigo_icc_linea15_tel")]
        [MaxLength(19)]
        public string? CodigoIccLinea15Tel { get; set; }

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
        public string? TipoAlarma { get; set; } // Hogar, Negocio

        [Column("subtipo_inmueble")]
        [MaxLength(100)]
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
        public string? KitAlarma { get; set; }

        [Column("opcionales_alarma")]
        [MaxLength(500)]
        public string? OpcionalesAlarma { get; set; }

        [Column("campana_alarma")]
        [MaxLength(255)]
        public string? CampanaAlarma { get; set; }

        [Column("empresa_alarma")]
        [MaxLength(255)]
        public string? EmpresaAlarma { get; set; }

        [Column("tipo_via_instalacion")]
        [MaxLength(50)]
        public string? TipoViaInstalacion { get; set; }

        [Column("direccion_instalacion_alarma")]
        [MaxLength(500)]
        public string? DireccionInstalacionAlarma { get; set; }

        [Column("numero_instalacion")]
        [MaxLength(20)]
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
        public string? CodigoPostalInstalacion { get; set; }

        [Column("provincia_instalacion")]
        [MaxLength(100)]
        public string? ProvinciaInstalacion { get; set; }

        [Column("localidad_instalacion")]
        [MaxLength(100)]
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

        // Campos Fibra Segunda Residencia (Telefonía)
        [Column("direccion_segunda_residencia")]
        [MaxLength(500)]
        public string? DireccionSegundaResidencia { get; set; }

        [Column("tarifa_fibra_segunda_residencia")]
        [MaxLength(255)]
        public string? TarifaFibraSegundaResidencia { get; set; }

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
