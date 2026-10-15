using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public sealed class CampoContratoEdicionMasiva
{
    private readonly PropertyInfo propiedad;
    private readonly Type tipoValor;

    public CampoContratoEdicionMasiva(string nombre, string etiqueta)
    {
        propiedad = typeof(Contrato).GetProperty(nombre)
            ?? throw new ArgumentException($"Campo de contrato desconocido: {nombre}", nameof(nombre));
        tipoValor = Nullable.GetUnderlyingType(propiedad.PropertyType) ?? propiedad.PropertyType;
        if (tipoValor != typeof(string) && tipoValor != typeof(int)
            && tipoValor != typeof(DateTime) && tipoValor != typeof(bool))
            throw new ArgumentException($"Tipo de campo no compatible: {nombre}", nameof(nombre));
        Nombre = nombre;
        Etiqueta = etiqueta;
        MaxLength = propiedad.GetCustomAttribute<MaxLengthAttribute>()?.Length;
        TipoEntrada = tipoValor == typeof(DateTime) ? "date"
            : tipoValor == typeof(bool) ? "boolean"
            : tipoValor == typeof(int) ? "number" : "text";
    }

    public string Nombre { get; }
    public string Etiqueta { get; }
    public int? MaxLength { get; }
    public string TipoEntrada { get; }
    public bool PermiteVacio => !propiedad.PropertyType.IsValueType
        || Nullable.GetUnderlyingType(propiedad.PropertyType) != null;

    public string Leer(Contrato contrato) => propiedad.GetValue(contrato) switch
    {
        null => "",
        DateTime fecha => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        bool valor => valor ? "1" : "0",
        IFormattable valor => valor.ToString(null, CultureInfo.InvariantCulture),
        var valor => valor.ToString() ?? ""
    };

    public object? Convertir(string texto)
    {
        texto = texto.Trim();
        if (tipoValor == typeof(string))
        {
            if (MaxLength.HasValue && texto.Length > MaxLength.Value)
                throw new FormatException($"{Etiqueta}: máximo {MaxLength.Value} caracteres.");
            return texto;
        }

        if (texto.Length == 0)
            return PermiteVacio ? null : false;

        if (tipoValor == typeof(int) && int.TryParse(texto, NumberStyles.None,
                CultureInfo.InvariantCulture, out var id) && id > 0)
            return id;
        if (tipoValor == typeof(DateTime) && DateTime.TryParseExact(texto,
                new[] { "yyyy-MM-dd", "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var fecha))
            return fecha;
        if (tipoValor == typeof(bool))
        {
            if (texto == "1" || texto.Equals("true", StringComparison.OrdinalIgnoreCase)
                || texto.Equals("sí", StringComparison.OrdinalIgnoreCase)
                || texto.Equals("si", StringComparison.OrdinalIgnoreCase))
                return true;
            if (texto == "0" || texto.Equals("false", StringComparison.OrdinalIgnoreCase)
                || texto.Equals("no", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        throw new FormatException($"{Etiqueta}: valor no válido «{texto}».");
    }

    public void Asignar(Contrato contrato, string texto) => propiedad.SetValue(contrato, Convertir(texto));

    internal void AsignarConvertido(Contrato contrato, object? valor) => propiedad.SetValue(contrato, valor);
    internal void Copiar(Contrato origen, Contrato destino) => propiedad.SetValue(destino, propiedad.GetValue(origen));
}

public static class CamposContratoEdicionMasiva
{
    private static readonly CampoContratoEdicionMasiva[] Comunes =
    {
        new(nameof(Contrato.FechaAlta), "Fecha Alta"),
        new(nameof(Contrato.IdContratoExterno), "ID Contrato Externo"),
        new(nameof(Contrato.CargadoEnPlataforma), "Cargado en Plataforma"),
        new(nameof(Contrato.IdCliente), "Cliente ID"),
        new(nameof(Contrato.TitularIbanDiferente), "Titular IBAN Diferente"),
        new(nameof(Contrato.TitularIbanDni), "DNI Titular IBAN"),
        new(nameof(Contrato.TitularIbanNombre), "Nombre Titular IBAN"),
        new(nameof(Contrato.TitularIbanNumero), "IBAN Otro Titular")
    };

    private static readonly CampoContratoEdicionMasiva[] DireccionInstalacion =
    {
        new(nameof(Contrato.TipoViaInstalacion), "Tipo Vía Instalación"),
        new(nameof(Contrato.DireccionInstalacionAlarma), "Dirección Instalación"),
        new(nameof(Contrato.NumeroInstalacion), "Número Instalación"),
        new(nameof(Contrato.EscaleraInstalacion), "Escalera Instalación"),
        new(nameof(Contrato.PisoInstalacion), "Piso Instalación"),
        new(nameof(Contrato.PuertaInstalacion), "Puerta Instalación"),
        new(nameof(Contrato.CodigoPostalInstalacion), "CP Instalación"),
        new(nameof(Contrato.AclaradorInstalacion), "Aclarador Instalación"),
        new(nameof(Contrato.LocalidadInstalacion), "Localidad Instalación"),
        new(nameof(Contrato.ProvinciaInstalacion), "Provincia Instalación")
    };

    private static readonly IReadOnlyList<CampoContratoEdicionMasiva> Energia = Comunes
        .Concat(DireccionInstalacion)
        .Concat(new CampoContratoEdicionMasiva[]
        {
            new(nameof(Contrato.ServicioId), "Servicio Luz ID"),
            new(nameof(Contrato.EstadoServicio), "Estado Servicio Luz"),
            new(nameof(Contrato.ServicioGasId), "Servicio Gas ID"),
            new(nameof(Contrato.EstadoServicioGas), "Estado Servicio Gas"),
            new(nameof(Contrato.ObservacionesEstado), "Observaciones Estado")
        }).ToArray();

    private static readonly IReadOnlyList<CampoContratoEdicionMasiva> Telefonia = CrearTelefonia();

    private static readonly IReadOnlyList<CampoContratoEdicionMasiva> Alarmas = Comunes
        .Concat(new CampoContratoEdicionMasiva[]
        {
            new(nameof(Contrato.TipoViaInstalacion), "Tipo Vía Instalación"),
            new(nameof(Contrato.TieneContratoAnterior), "Tiene Contrato Anterior"),
            new(nameof(Contrato.CompaniaAnterior), "Compañía Anterior"),
            new(nameof(Contrato.NumeroContratoAnterior), "Número Contrato Anterior"),
            new(nameof(Contrato.FechaPermanenciaAnterior), "Permanencia Anterior")
        }).ToArray();

    private static IReadOnlyList<CampoContratoEdicionMasiva> CrearTelefonia()
    {
        var campos = Comunes.Concat(DireccionInstalacion).Concat(new CampoContratoEdicionMasiva[]
        {
            new(nameof(Contrato.TipoTarifaTel), "Tipo Tarifa"),
            new(nameof(Contrato.AltaNuevaLineaPrincipal), "Alta Nueva Móvil 1"),
            new(nameof(Contrato.AltaNuevaLineaPrincipal2), "Alta Nueva Móvil 2"),
            new(nameof(Contrato.TarifaFibraSegundaResidencia), "Tarifa Fibra Segunda Residencia"),
            new(nameof(Contrato.DireccionSegundaResidencia), "Dirección Segunda Residencia"),
            new(nameof(Contrato.ObservacionesEstado), "Observaciones Estado")
        }).ToList();

        for (var linea = 1; linea <= 15; linea++)
        {
            campos.Add(new($"TelefonoLinea{linea}Tel", $"Línea Adicional {linea}"));
            campos.Add(new($"TarifaLinea{linea}Tel", $"Tarifa Adicional {linea}"));
            campos.Add(new($"TipoLinea{linea}Tel", $"Tipo Adicional {linea}"));
            campos.Add(new($"CodigoIccLinea{linea}Tel", $"ICC Adicional {linea}"));
            campos.Add(new($"AltaNuevaLinea{linea}Tel", $"Alta Nueva Adicional {linea}"));
        }
        return campos;
    }

    public static IReadOnlyList<CampoContratoEdicionMasiva> Obtener(string tipo) => tipo switch
    {
        "energia" => Energia,
        "telefonia" => Telefonia,
        "alarma" => Alarmas,
        _ => throw new ArgumentException($"Tipo de contrato no compatible: {tipo}", nameof(tipo))
    };

    public static int ColumnasOriginales(string tipo) => tipo switch
    {
        "energia" => 27,
        "telefonia" => 22,
        "alarma" => 25,
        _ => throw new ArgumentException($"Tipo de contrato no compatible: {tipo}", nameof(tipo))
    };

    public static string Cabeceras(string tipo) => string.Join("\t", Obtener(tipo).Select(c => c.Nombre));

    public static string Valores(Contrato contrato, string tipo) =>
        string.Join("\t", Obtener(tipo).Select(c => c.Leer(contrato).Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ')));

    public static void Aplicar(Contrato contrato, string tipo, IReadOnlyList<string> columnas)
    {
        var campos = Obtener(tipo);
        var inicio = ColumnasOriginales(tipo);
        var cantidad = Math.Clamp(columnas.Count - inicio, 0, campos.Count);
        // Validar toda la extensión antes de modificar la fila.
        var valores = Enumerable.Range(0, cantidad).Select(i => campos[i].Convertir(columnas[inicio + i])).ToArray();
        for (var i = 0; i < cantidad; i++)
            campos[i].AsignarConvertido(contrato, valores[i]);
    }

    public static void Copiar(Contrato origen, Contrato destino, string tipo, int? numeroColumnas = null)
    {
        var campos = Obtener(tipo);
        var cantidad = numeroColumnas.HasValue
            ? Math.Clamp(numeroColumnas.Value - ColumnasOriginales(tipo), 0, campos.Count) : campos.Count;
        for (var i = 0; i < cantidad; i++)
            campos[i].Copiar(origen, destino);
    }

    public static async Task<List<string>> ValidarReferenciasAsync(List<Contrato> contratos, ApplicationDbContext context)
    {
        var clientes = contratos.Where(c => c.IdCliente.HasValue).Select(c => c.IdCliente!.Value).Distinct().ToArray();
        var servicios = contratos.Where(c => c.ServicioId.HasValue).Select(c => c.ServicioId!.Value).Distinct().ToArray();
        var serviciosGas = contratos.Where(c => c.ServicioGasId.HasValue).Select(c => c.ServicioGasId!.Value).Distinct().ToArray();
        var clientesValidos = clientes.Length == 0 ? new List<int>()
            : await context.Clientes.Where(c => clientes.Contains(c.Id)).Select(c => c.Id).ToListAsync();
        var serviciosValidos = servicios.Length == 0 ? new List<int>()
            : await context.Servicios.Where(s => servicios.Contains(s.Id)).Select(s => s.Id).ToListAsync();
        var serviciosGasValidos = serviciosGas.Length == 0 ? new List<int>()
            : await context.ServiciosGas.Where(s => serviciosGas.Contains(s.Id)).Select(s => s.Id).ToListAsync();
        var errores = new List<string>();
        for (var i = 0; i < contratos.Count; i++)
        {
            var contrato = contratos[i];
            if (contrato.IdCliente.HasValue && !clientesValidos.Contains(contrato.IdCliente.Value))
                errores.Add($"Fila {i + 1}: El Cliente ID {contrato.IdCliente} no existe.");
            if (contrato.ServicioId.HasValue && !serviciosValidos.Contains(contrato.ServicioId.Value))
                errores.Add($"Fila {i + 1}: El Servicio Luz ID {contrato.ServicioId} no existe.");
            if (contrato.ServicioGasId.HasValue && !serviciosGasValidos.Contains(contrato.ServicioGasId.Value))
                errores.Add($"Fila {i + 1}: El Servicio Gas ID {contrato.ServicioGasId} no existe.");
        }
        return errores;
    }
}
