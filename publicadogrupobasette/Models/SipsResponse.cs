using System.Text.Json.Serialization;

namespace EnerfoneCRM.Models
{
    public class SipsResponse
    {
        [JsonPropertyName("ClientesSips")]
        public List<ClienteSips>? ClientesSips { get; set; }

        [JsonPropertyName("ConsumosSips")]
        public List<ConsumoSips>? ConsumosSips { get; set; }

        [JsonPropertyName("DatosTitular")]
        public object? DatosTitular { get; set; }
    }

    public class ClienteSips
    {
        [JsonPropertyName("CodigoCUPS")]
        public string? CodigoCUPS { get; set; }

        [JsonPropertyName("CodigoEmpresaDistribuidora")]
        public string? CodigoEmpresaDistribuidora { get; set; }

        [JsonPropertyName("NombreEmpresaDistribuidora")]
        public string? NombreEmpresaDistribuidora { get; set; }

        [JsonPropertyName("CodigoPostalPS")]
        public string? CodigoPostalPS { get; set; }

        [JsonPropertyName("MunicipioPS")]
        public string? MunicipioPS { get; set; }

        [JsonPropertyName("CodigoProvinciaPS")]
        public string? CodigoProvinciaPS { get; set; }

        [JsonPropertyName("FechaAltaSuministro")]
        public DateTime? FechaAltaSuministro { get; set; }

        [JsonPropertyName("CodigoTarifaATREnVigor")]
        public string? CodigoTarifaATREnVigor { get; set; }

        [JsonPropertyName("CodigoTensionV")]
        public string? CodigoTensionV { get; set; }

        [JsonPropertyName("PotenciaMaximaBIEW")]
        public decimal? PotenciaMaximaBIEW { get; set; }

        [JsonPropertyName("PotenciaMaximaAPMW")]
        public decimal? PotenciaMaximaAPMW { get; set; }

        [JsonPropertyName("CodigoClasificacionPS")]
        public string? CodigoClasificacionPS { get; set; }

        [JsonPropertyName("CodigoDisponibilidadICP")]
        public string? CodigoDisponibilidadICP { get; set; }

        [JsonPropertyName("TipoPerfilConsumo")]
        public string? TipoPerfilConsumo { get; set; }

        [JsonPropertyName("ValorDerechosExtensionW")]
        public string? ValorDerechosExtensionW { get; set; }

        [JsonPropertyName("ValorDerechosAccesoW")]
        public string? ValorDerechosAccesoW { get; set; }

        [JsonPropertyName("CodigoPropiedadEquipoMedida")]
        public string? CodigoPropiedadEquipoMedida { get; set; }

        [JsonPropertyName("CodigoPropiedadICP")]
        public string? CodigoPropiedadICP { get; set; }

        [JsonPropertyName("PotenciasContratadasEnWP1")]
        public decimal? PotenciasContratadasEnWP1 { get; set; }

        [JsonPropertyName("PotenciasContratadasEnWP2")]
        public decimal? PotenciasContratadasEnWP2 { get; set; }

        [JsonPropertyName("PotenciasContratadasEnWP3")]
        public decimal? PotenciasContratadasEnWP3 { get; set; }

        [JsonPropertyName("PotenciasContratadasEnWP4")]
        public decimal? PotenciasContratadasEnWP4 { get; set; }

        [JsonPropertyName("PotenciasContratadasEnWP5")]
        public decimal? PotenciasContratadasEnWP5 { get; set; }

        [JsonPropertyName("PotenciasContratadasEnWP6")]
        public decimal? PotenciasContratadasEnWP6 { get; set; }

        [JsonPropertyName("FechaUltimoMovimientoContrato")]
        public DateTime? FechaUltimoMovimientoContrato { get; set; }

        [JsonPropertyName("FechaUltimoCambioComercializador")]
        public DateTime? FechaUltimoCambioComercializador { get; set; }

        [JsonPropertyName("FechaLimiteDerechosReconocidos")]
        public DateTime? FechaLimiteDerechosReconocidos { get; set; }

        [JsonPropertyName("FechaUltimaLectura")]
        public DateTime? FechaUltimaLectura { get; set; }

        [JsonPropertyName("InformacionImpagos")]
        public string? InformacionImpagos { get; set; }

        [JsonPropertyName("ImporteDepositoGarantiaEuros")]
        public string? ImporteDepositoGarantiaEuros { get; set; }

        [JsonPropertyName("TipoIdTitular")]
        public string? TipoIdTitular { get; set; }

        [JsonPropertyName("EsViviendaHabitual")]
        public string? EsViviendaHabitual { get; set; }

        [JsonPropertyName("CodigoComercializadora")]
        public string? CodigoComercializadora { get; set; }

        [JsonPropertyName("CodigoTelegestion")]
        public string? CodigoTelegestion { get; set; }

        [JsonPropertyName("CodigoFasesEquipoMedida")]
        public string? CodigoFasesEquipoMedida { get; set; }

        [JsonPropertyName("CodigoAutoconsumo")]
        public string? CodigoAutoconsumo { get; set; }

        [JsonPropertyName("CodigoTipoContrato")]
        public string? CodigoTipoContrato { get; set; }

        [JsonPropertyName("CodigoPeriodicidadFacturacion")]
        public string? CodigoPeriodicidadFacturacion { get; set; }

        [JsonPropertyName("Cnae")]
        public string? Cnae { get; set; }

        [JsonPropertyName("AplicacionBonoSocial")]
        public string? AplicacionBonoSocial { get; set; }

        [JsonPropertyName("DesProvinciaPS")]
        public string? DesProvinciaPS { get; set; }

        [JsonPropertyName("DesMunicipioPS")]
        public string? DesMunicipioPS { get; set; }

        [JsonPropertyName("TipoViaPS")]
        public string? TipoViaPS { get; set; }

        [JsonPropertyName("ViaPS")]
        public string? ViaPS { get; set; }

        [JsonPropertyName("NumFincaPS")]
        public string? NumFincaPS { get; set; }

        [JsonPropertyName("PortalPS")]
        public string? PortalPS { get; set; }

        [JsonPropertyName("EscaleraPS")]
        public string? EscaleraPS { get; set; }

        [JsonPropertyName("PisoPS")]
        public string? PisoPS { get; set; }

        [JsonPropertyName("PuertaPS")]
        public string? PuertaPS { get; set; }
    }

    public class ConsumoSips
    {
        [JsonPropertyName("CodigoCUPS")]
        public string? CodigoCUPS { get; set; }

        [JsonPropertyName("FechaInicio")]
        public DateTime? FechaInicio { get; set; }

        [JsonPropertyName("FechaFin")]
        public DateTime? FechaFin { get; set; }

        [JsonPropertyName("CodigoTarifaATR")]
        public string? CodigoTarifaATR { get; set; }

        [JsonPropertyName("Activa1")]
        public decimal? Activa1 { get; set; }

        [JsonPropertyName("Activa2")]
        public decimal? Activa2 { get; set; }

        [JsonPropertyName("Activa3")]
        public decimal? Activa3 { get; set; }

        [JsonPropertyName("Activa4")]
        public decimal? Activa4 { get; set; }

        [JsonPropertyName("Activa5")]
        public decimal? Activa5 { get; set; }

        [JsonPropertyName("Activa6")]
        public decimal? Activa6 { get; set; }

        [JsonPropertyName("Reactiva1")]
        public decimal? Reactiva1 { get; set; }

        [JsonPropertyName("Reactiva2")]
        public decimal? Reactiva2 { get; set; }

        [JsonPropertyName("Reactiva3")]
        public decimal? Reactiva3 { get; set; }

        [JsonPropertyName("Reactiva4")]
        public decimal? Reactiva4 { get; set; }

        [JsonPropertyName("Reactiva5")]
        public decimal? Reactiva5 { get; set; }

        [JsonPropertyName("Reactiva6")]
        public decimal? Reactiva6 { get; set; }

        [JsonPropertyName("Potencia1")]
        public decimal? Potencia1 { get; set; }

        [JsonPropertyName("Potencia2")]
        public decimal? Potencia2 { get; set; }

        [JsonPropertyName("Potencia3")]
        public decimal? Potencia3 { get; set; }

        [JsonPropertyName("Potencia4")]
        public decimal? Potencia4 { get; set; }

        [JsonPropertyName("Potencia5")]
        public decimal? Potencia5 { get; set; }

        [JsonPropertyName("Potencia6")]
        public decimal? Potencia6 { get; set; }

        public decimal TotalConsumo => (Activa1 ?? 0) + (Activa2 ?? 0) + (Activa3 ?? 0) + 
                                        (Activa4 ?? 0) + (Activa5 ?? 0) + (Activa6 ?? 0);
    }
}
