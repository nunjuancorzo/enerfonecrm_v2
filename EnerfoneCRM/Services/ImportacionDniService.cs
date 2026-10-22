using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EnerfoneCRM.Services;

public sealed class ImportacionDniService
{
    private const int MaximoBytes = 10 * 1024 * 1024;
    private const int MaximoCaracteresOcr = 100_000;
    private const string LetrasDni = "TRWAGMYFPDXBNJZSQVHLCKE";
    private readonly OcrService ocrService;

    public ImportacionDniService(OcrService ocrService) => this.ocrService = ocrService;

    public async Task<ResultadoImportacionDni> LeerAsync(byte[] archivo, string nombreArchivo)
    {
        if (archivo.Length == 0 || archivo.Length > MaximoBytes)
            return new(false, "El archivo está vacío o supera los 10 MB.");

        try
        {
            var mime = OcrService.ObtenerTipoDocumento(archivo);
            var extension = mime switch
            {
                "application/pdf" => ".pdf",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
            var resultadoOcr = await ocrService.ProcesarDocumentoIdentidadLocalAsync(archivo, "documento" + extension);
            if (!resultadoOcr.Exito || string.IsNullOrWhiteSpace(resultadoOcr.TextoCompleto))
                return new(false, resultadoOcr.Mensaje);

            if (resultadoOcr.TextoCompleto.Length > MaximoCaracteresOcr)
                return new(false, "El documento contiene demasiado texto para analizarlo.");

            var datos = ExtraerDatos(resultadoOcr.TextoCompleto);
            return datos == null
                ? new(false, "No se ha podido identificar un DNI o NIE válido. Prueba con una imagen más nítida, completa y sin reflejos.")
                : new(true, datos.CorregidoPorOcr
                    ? "Se corrigieron caracteres OCR ambiguos usando el checksum. Revisa especialmente el número antes de guardar."
                    : "Datos leídos localmente. Revisa y corrige los campos antes de guardar.", datos);
        }
        catch (ArgumentException excepcion)
        {
            return new(false, excepcion.Message);
        }
        catch
        {
            return new(false, "No se ha podido leer el documento. Comprueba su formato y vuelve a intentarlo.");
        }
    }

    public static DatosDocumentoIdentidad? ExtraerDatos(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto) || texto.Length > MaximoCaracteresOcr)
            return null;

        var documento = ExtraerDocumento(texto, out var corregidoPorOcr);
        if (documento == null)
            return null;

        var nombre = ExtraerCampoEtiquetado(texto, TipoEtiqueta.Nombre);
        var apellidos = ExtraerCampoEtiquetado(texto, TipoEtiqueta.Apellidos);
        var nacionalidad = ExtraerCampoEtiquetado(texto, TipoEtiqueta.Nacionalidad);
        var direccion = ExtraerCampoEtiquetado(texto, TipoEtiqueta.Direccion);
        var codigoPostal = direccion == null
            ? null
            : Regex.Match(direccion, @"(?<!\d)\d{5}(?!\d)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Value;

        return new DatosDocumentoIdentidad(
            documento,
            documento[0] is 'X' or 'Y' or 'Z' ? "NIE" : "DNI",
            nombre,
            apellidos,
            NormalizarNacionalidad(nacionalidad),
            direccion,
            string.IsNullOrEmpty(codigoPostal) ? null : codigoPostal,
            corregidoPorOcr);
    }

    private static string? ExtraerDocumento(string texto, out bool corregidoPorOcr)
    {
        corregidoPorOcr = false;
        var candidatos = new List<string>();
        const string documentoOcr = @"[XYZ]?\s*(?:[0-9OQDI|LSB][\s./-]*){7,8}[A-Z]";
        var mrz = Regex.Match(texto, $@"\bIDESP\s*(?<documento>{documentoOcr})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        if (mrz.Success)
            candidatos.Add(mrz.Groups["documento"].Value);

        var etiquetado = Regex.Match(texto, $@"\b(?:DNI|NIE|NIF|NUM(?:ERO)?(?:\s+DE)?\s+IDENTIDAD)\s*[:#\-]?\s*(?<documento>{documentoOcr})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        if (etiquetado.Success)
            candidatos.Add(etiquetado.Groups["documento"].Value);

        candidatos.AddRange(Regex.Matches(texto, $@"(?<![A-Z0-9])(?<documento>{documentoOcr})(?![A-Z0-9])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1))
            .Select(coincidencia => coincidencia.Groups["documento"].Value));

        foreach (var candidato in candidatos)
        {
            var documento = Regex.Replace(candidato, @"[\s./-]+", string.Empty).ToUpperInvariant();
            if (EsDocumentoValido(documento))
                return documento;

            var candidatoCorregido = CorregirCaracteresOcr(documento);
            if (candidatoCorregido != documento && EsDocumentoValido(candidatoCorregido))
            {
                corregidoPorOcr = true;
                return candidatoCorregido;
            }
        }

        return null;
    }

    private static string CorregirCaracteresOcr(string documento)
    {
        if (documento.Length == 0) return documento;
        var inicioDigitos = documento[0] is 'X' or 'Y' or 'Z' ? 1 : 0;
        var caracteres = documento.ToCharArray();
        for (var indice = inicioDigitos; indice < caracteres.Length - 1; indice++)
        {
            caracteres[indice] = caracteres[indice] switch
            {
                'O' or 'Q' or 'D' => '0',
                'I' or 'L' or '|' => '1',
                'S' => '5',
                'B' => '8',
                _ => caracteres[indice]
            };
        }
        return new string(caracteres);
    }

    private static bool EsDocumentoValido(string documento)
    {
        if (documento.Length != 9)
            return false;

        var posicion = documento[0] switch
        {
            'X' => 0,
            'Y' => 1,
            'Z' => 2,
            _ => -1
        };
        var digitos = posicion >= 0 ? posicion.ToString(CultureInfo.InvariantCulture) + documento[1..^1] : documento[..^1];
        if (!int.TryParse(digitos, NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
            return false;

        return documento[^1] == LetrasDni[numero % LetrasDni.Length];
    }

    private static string? ExtraerCampoEtiquetado(string texto, TipoEtiqueta tipo)
    {
        var lineas = texto.Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var indice = 0; indice < lineas.Length; indice++)
        {
            var coincidencia = Regex.Match(lineas[indice], tipo.Patron, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
            if (!coincidencia.Success)
                continue;

            var valor = LimpiarValor(coincidencia.Groups["valor"].Value);
            if (valor != null)
                return valor;

            for (var siguiente = indice + 1; siguiente < Math.Min(indice + 3, lineas.Length); siguiente++)
            {
                if (EsEtiqueta(lineas[siguiente]))
                    break;

                valor = LimpiarValor(lineas[siguiente]);
                if (valor != null)
                    return valor;
            }
        }

        return null;
    }

    private static bool EsEtiqueta(string linea) =>
        Regex.IsMatch(linea, @"^\s*(?:APELLIDOS?|SURNAMES?|NOMBRES?|GIVEN NAMES?|NACIONALIDAD|NATIONALITY|DOMICILIO|DIRECCI[ÓO]N|ADDRESS|FECHA|DATE|SEXO|SEX|VALIDEZ|EXPEDICI[ÓO]N)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static string? LimpiarValor(string valor)
    {
        var limpio = Regex.Replace(valor, @"\s+", " ").Trim(' ', ':', '-', '/', '<', '>');
        if (limpio.Length < 2 || limpio.Length > 120 || EsEtiqueta(limpio))
            return null;
        return limpio;
    }

    private static string? NormalizarNacionalidad(string? nacionalidad)
    {
        if (string.IsNullOrWhiteSpace(nacionalidad))
            return null;
        return nacionalidad.Trim().ToUpperInvariant() switch
        {
            "ESP" or "ESPAÑA" or "ESPAÑOLA" or "ESPANOLA" => "Española",
            _ => nacionalidad.Trim()
        };
    }

    private sealed record TipoEtiqueta(string Patron)
    {
        public static TipoEtiqueta Nombre { get; } = new(@"^\s*(?:NOMBRE|GIVEN NAMES?)(?:\s*/\s*NAME)?\s*[:\-]?\s*(?<valor>.*)$");
        public static TipoEtiqueta Apellidos { get; } = new(@"^\s*(?:1(?:ER|[ºO])?\s*)?APELLIDOS?(?:\s*/\s*SURNAMES?)?\s*[:\-]?\s*(?<valor>.*)$");
        public static TipoEtiqueta Nacionalidad { get; } = new(@"^\s*(?:NACIONALIDAD|NATIONALITY)\s*[:\-]?\s*(?<valor>.*)$");
        public static TipoEtiqueta Direccion { get; } = new(@"^\s*(?:DOMICILIO|DIRECCI[ÓO]N|ADDRESS)\s*[:\-]?\s*(?<valor>.*)$");
    }
}

public sealed record DatosDocumentoIdentidad(
    string Documento,
    string TipoDocumento,
    string? Nombre,
    string? Apellidos,
    string? Nacionalidad,
    string? Direccion,
    string? CodigoPostal,
    bool CorregidoPorOcr = false)
{
    public string NombreCompleto => string.Join(' ', new[] { Nombre, Apellidos }.Where(valor => !string.IsNullOrWhiteSpace(valor)));
}

public sealed record ResultadoImportacionDni(bool Exito, string Mensaje, DatosDocumentoIdentidad? Datos = null);