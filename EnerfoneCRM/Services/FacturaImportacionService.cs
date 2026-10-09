using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using EnerfoneCRM.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace EnerfoneCRM.Services;

public class FacturaImportacionService
{
    public const int MaximoBytes = 10 * 1024 * 1024;
    public const int MaximoPaginas = 20;
    private const string Numero = @"[-+]?\d+(?:[.,]\d+)*";
    private const string Fecha = @"(?:\d{1,2}[/.-]\d{1,2}[/.-]\d{4}|\d{4}-\d{2}-\d{2}|\d{1,2}\s+de\s+[a-z]+\s+de\s+\d{4})";
    private readonly OcrService ocrService;
    public static IReadOnlyList<string> ClavesExtraccion => CrearResultado().Campos.Select(campo => campo.Clave).ToList();

    public FacturaImportacionService(OcrService ocrService) => this.ocrService = ocrService;

    public async Task<FacturaImportada> ExtraerAsync(byte[] archivo, string nombre, bool permitirProveedorExterno = false)
    {
        if (archivo.Length == 0 || archivo.Length > MaximoBytes)
            throw new ArgumentException("El archivo está vacío o supera los 10 MB.");
        var mime = OcrService.ObtenerTipoDocumento(archivo);
        var esPdf = mime == "application/pdf";
        var nombreSeguro = mime switch { "application/pdf" => "factura.pdf", "image/png" => "factura.png", "image/webp" => "factura.webp", _ => "factura.jpg" };
        var texto = new StringBuilder();
        var necesitaOcr = !esPdf;
        if (esPdf)
        {
            using var documento = PdfDocument.Open(archivo);
            if (documento.NumberOfPages > MaximoPaginas)
                throw new ArgumentException("La factura supera el límite de 20 páginas.");
            foreach (var pagina in documento.GetPages())
            {
                var textoPagina = ContentOrderTextExtractor.GetText(pagina);
                necesitaOcr |= textoPagina.Count(char.IsLetterOrDigit) < 30;
                texto.AppendLine(textoPagina);
                if (texto.Length > 200000) throw new ArgumentException("El documento contiene demasiado texto para una factura.");
            }
        }

        var resultado = AnalizarTexto(texto.ToString());
        resultado.Origen = "PDF local";
        if (necesitaOcr)
        {
            var local = await ocrService.ProcesarLocalAsync(archivo, nombreSeguro);
            if (local.Exito && !string.IsNullOrWhiteSpace(local.TextoCompleto))
            {
                texto.AppendLine(local.TextoCompleto);
                resultado = AnalizarTexto(texto.ToString());
                resultado.Origen = "OCR local";
            }
            else resultado.Advertencias.Add("No se ha podido leer la parte escaneada. Comprueba Tesseract, el idioma español y pdftoppm en el servidor.");
        }

        if (permitirProveedorExterno)
        {
            var externo = await ocrService.ProcesarFacturaAsync(archivo, nombreSeguro);
            if (externo.Exito && externo.ProveedorUtilizado != "tesseract")
            {
                if (!string.IsNullOrWhiteSpace(externo.TextoCompleto))
                {
                    var datosTexto = AnalizarTexto(externo.TextoCompleto).Campos.Where(campo => campo.Seleccionado)
                        .ToDictionary(campo => campo.Clave, campo => campo.Valor);
                    IncorporarDatos(resultado, datosTexto, "Texto del proveedor " + externo.ProveedorUtilizado);
                }
                IncorporarDatos(resultado, externo.DatosExtraidos, "Proveedor " + externo.ProveedorUtilizado);
                resultado.Origen += " + " + externo.ProveedorUtilizado;
            }
            else resultado.Advertencias.Add("El proveedor externo configurado no ha aportado datos estructurados. Se conservan los resultados locales.");
        }
        Revisar(resultado);
        return resultado;
    }

    public static FacturaImportada AnalizarTexto(string texto)
    {
        if (texto.Length > 200000) throw new ArgumentException("Texto de factura demasiado largo.");
        var resultado = CrearResultado();
        resultado.Texto = texto;
        var normalizado = Normalizar(texto);
        var lineasOriginales = texto.Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var lineas = lineasOriginales.Select(Normalizar).ToArray();
        var cups = Coincidir(normalizado, @"\bES\d{16}[A-Z]{2}(?:[A-Z0-9]{2})?\b");
        if (cups.Success) Asignar(resultado, "cups", cups.Value.ToUpperInvariant(), "CUPS localizado en el documento");
        var peajeLuz = Coincidir(normalizado, @"\b(?:2\.0|3\.0|6\.[1-4])\s*TD\b");
        var peajeGas = Coincidir(normalizado, @"\bRL\.?\s*\d{1,2}\b");
        if (peajeLuz.Success) Asignar(resultado, "peaje_luz", Regex.Replace(peajeLuz.Value, @"\s", "").ToUpperInvariant(), peajeLuz.Value);
        if (peajeGas.Success) Asignar(resultado, "peaje_gas", Regex.Replace(peajeGas.Value, @"\s", "").ToUpperInvariant(), peajeGas.Value);
        if (peajeLuz.Success || peajeGas.Success)
            Asignar(resultado, "tipo_suministro", peajeLuz.Success && peajeGas.Success ? "Mixto" : peajeGas.Success ? "Gas" : "Luz", "Peajes del documento");
        else if (Coincidir(normalizado, @"consumo\s+(?:electrico|de electricidad)|factura\s+(?:de\s+)?(?:luz|electricidad)").Success)
            Asignar(resultado, "tipo_suministro", "Luz", "Conceptos eléctricos");
        else if (Coincidir(normalizado, @"consumo\s+de\s+gas|factura\s+(?:de\s+)?gas").Success)
            Asignar(resultado, "tipo_suministro", "Gas", "Conceptos de gas");

        for (var indiceLinea = 0; indiceLinea < lineas.Length; indiceLinea++)
        {
            var linea = lineas[indiceLinea];
            var original = lineasOriginales[indiceLinea];
            foreach (var datoTexto in new[]
            {
                ("comercializadora_actual", @"(?:comercializadora|proveedor|empresa emisora|compa[nñ][ií]a actual)"),
                ("nombre_cliente", @"(?:titular|nombre del (?:cliente|titular))"),
                ("direccion_cliente", @"direcci[oó]n(?: de suministro)?"),
                ("email_cliente", @"(?:email|correo electr[oó]nico)")
            })
            {
                var dato = Coincidir(original, $@"^\s*{datoTexto.Item2}\s*:\s*(?<texto>[^\r\n]{{2,150}})$");
                if (dato.Success) Asignar(resultado, datoTexto.Item1, dato.Groups["texto"].Value.Trim(), original);
            }
            var periodo = Coincidir(linea, $@"(?:periodo[^\d]{{0,70}}|desde\s+)(?<inicio>{Fecha})\s*(?:al?|hasta|-)\s*(?<fin>{Fecha})");
            if (periodo.Success)
            {
                Asignar(resultado, "fecha_inicio", periodo.Groups["inicio"].Value, linea);
                Asignar(resultado, "fecha_fin", periodo.Groups["fin"].Value, linea);
            }
            ExtraerFecha(resultado, linea, "fecha_inicio", "(?:fecha (?:de )?inicio|desde)");
            ExtraerFecha(resultado, linea, "fecha_fin", "(?:fecha (?:de )?fin|hasta)");
            ExtraerNumero(resultado, linea, "total_factura", "(?:total (?:a pagar|factura|de (?:la )?factura)|importe total|importe a pagar|total)\\s*[:=]?", @"\s*(?:€|eur|euros|$)");
            ExtraerNumero(resultado, linea, "consumo_total", "(?:consumo total|consumo (?:de gas|facturado)|energia consumida)\\s*[:=]?", @"\s*kwh\b");
            ExtraerNumero(resultado, linea, "iva_porcentaje", "iva\\s*[:(]?", @"\s*%");
            ExtraerNumero(resultado, linea, "impuesto_electricidad_porcentaje", "impuesto (?:sobre (?:la )?)?electricidad\\s*[:(]?", @"\s*%");
            ExtraerNumero(resultado, linea, "alquiler_contador", "(?:alquiler (?:de )?(?:contador|equipos)|equipos de medida)\\s*[:=]?", @"\s*(?:€|eur|euros|$)");
            ExtraerNumero(resultado, linea, "excedentes_kwh", "(?:excedentes|energia excedentaria)\\s*[:=]?", @"\s*kwh\b");
            ExtraerNumero(resultado, linea, "termino_fijo_gas", "termino fijo(?: gas)?\\s*[:=]?", @"\s*(?:€|eur)?\s*/\s*(?:dia|mes)");
            ExtraerNumero(resultado, linea, "termino_variable_gas", "termino variable(?: gas)?\\s*[:=]?", @"\s*(?:€|eur)?\s*/\s*kwh\b");
            ExtraerNumero(resultado, linea, "precio_energia_medio", "(?:precio medio|consumo[^\\d]{0,35})", @"\s*(?:€|eur)\s*/\s*kwh\b");
            foreach (var campo in new[] { ("exceso_potencia", "exceso de potencia"), ("energia_reactiva", "energia reactiva"), ("bono_social", "financiacion (?:del )?bono social"), ("descuento_importe", "descuentos?") })
                ExtraerNumero(resultado, linea, campo.Item1, campo.Item2 + "\\s*[:=]?", @"\s*(?:€|eur|euros|$)");

            for (var indice = 1; indice <= 6; indice++)
            {
                var periodoNombre = $@"(?:P{indice}|periodo\s*{indice})";
                ExtraerNumero(resultado, linea, $"potencia_p{indice}", $@"(?:potencia(?: contratada)?\s*)?{periodoNombre}\s*[:=]?", @"\s*kw\b(?!h|\s*/)");
                ExtraerNumero(resultado, linea, $"consumo_p{indice}", $@"(?:(?:consumo|energia)\s*)?{periodoNombre}\s*[:=]?", @"\s*kwh\b");
                ExtraerNumero(resultado, linea, $"precio_potencia_p{indice}", $@"(?:precio\s+)?potencia\s*{periodoNombre}\s*[:=]?", @"\s*(?:€|eur)\s*/\s*kw\s*(?:/|·)\s*(?:dia|mes|ano)");
                ExtraerNumero(resultado, linea, $"precio_energia_p{indice}", $@"(?:precio\s+)?(?:energia|consumo)\s*{periodoNombre}\s*[:=]?", @"\s*(?:€|eur)\s*/\s*kwh\b");
                foreach (var magnitud in new[] { ("precio_potencia", "kw", @"kw\s*(?:/|·)\s*(?:dia|mes|ano)"), ("precio_energia", "kwh", "kwh") })
                {
                    var formula = Coincidir(linea, $@"{periodoNombre}\s*[:=]?\s*{Numero}\s*{magnitud.Item2}\s*(?:x|\*)\s*(?<precio>{Numero})\s*(?:€|eur)\s*/\s*{magnitud.Item3}");
                    if (formula.Success) Asignar(resultado, $"{magnitud.Item1}_p{indice}", formula.Groups["precio"].Value, linea);
                }
            }
            foreach (var periodoConsumo in new[] { ("punta", 1), ("llano", 2), ("valle", 3) })
            {
                ExtraerNumero(resultado, linea, $"consumo_p{periodoConsumo.Item2}", $@"consumo\s*{periodoConsumo.Item1}\s*[:=]?", @"\s*kwh\b");
                ExtraerNumero(resultado, linea, $"precio_energia_p{periodoConsumo.Item2}", $@"(?:precio\s+)?(?:consumo|energia)\s*{periodoConsumo.Item1}\s*[:=]?", @"\s*(?:€|eur)\s*/\s*kwh\b");
            }
            ExtraerNumero(resultado, linea, "potencia_p1", "potencia(?: contratada)? punta\\s*[:=]?", @"\s*kw\b(?!h|\s*/)");
            ExtraerNumero(resultado, linea, "potencia_p2", "potencia(?: contratada)? valle\\s*[:=]?", @"\s*kw\b(?!h|\s*/)");
            var unidad = Coincidir(linea, @"(?:€|eur)\s*/\s*kw\s*(?:/|·)\s*(?<unidad>dia|mes|ano)");
            if (unidad.Success) Asignar(resultado, "unidad_potencia", unidad.Groups["unidad"].Value, linea);
        }
        Revisar(resultado);
        return resultado;
    }

    public static void IncorporarDatos(FacturaImportada resultado, IReadOnlyDictionary<string, string> datos, string origen)
    {
        foreach (var dato in datos)
        {
            var clave = dato.Key switch { "fecha_inicio_periodo" => "fecha_inicio", "fecha_fin_periodo" => "fecha_fin", "tipo_energia" => "tipo_suministro", "consumo_gas" => "consumo_total", "iva" => "iva_porcentaje", _ => dato.Key };
            if (resultado.Campos.Any(campo => campo.Clave == clave) && !string.IsNullOrWhiteSpace(dato.Value) && dato.Value != "null")
                Asignar(resultado, clave, dato.Value, origen);
        }
    }

    public static bool TryNumero(string? texto, out decimal valor)
    {
        valor = 0;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        var limpio = texto.Trim().Replace(" ", "").Replace("\u00a0", "");
        if (!Regex.IsMatch(limpio, @"^[-+]?\d+(?:[.,]\d+)*$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1))) return false;
        if (limpio.Contains(',') && limpio.Contains('.'))
        {
            var decimalComa = limpio.LastIndexOf(',') > limpio.LastIndexOf('.');
            limpio = decimalComa ? limpio.Replace(".", "").Replace(',', '.') : limpio.Replace(",", "");
        }
        else if (Regex.IsMatch(limpio, @"^[1-9]\d*\.\d{3}$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
            return false;
        else limpio = limpio.Replace(',', '.');
        return decimal.TryParse(limpio, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out valor);
    }

    public static bool TryFecha(string? texto, out DateOnly fecha) => DateOnly.TryParseExact(texto?.Trim(),
        new[] { "yyyy-MM-dd", "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "dd-MM-yyyy", "d.M.yyyy", "dd.MM.yyyy", "d 'de' MMMM 'de' yyyy" },
        CultureInfo.GetCultureInfo("es-ES"), DateTimeStyles.None, out fecha);

    public static List<string> ValidarParaAplicar(FacturaImportada factura)
    {
        var errores = new List<string>();
        var seleccionados = factura.Campos.Where(campo => campo.Seleccionado && !string.IsNullOrWhiteSpace(campo.Valor)).ToList();
        if (seleccionados.Count == 0) errores.Add("Selecciona al menos un dato de la factura.");
        foreach (var campo in seleccionados)
        {
            if (EsNumerico(campo.Clave) && (!TryNumero(campo.Valor, out var numero) || (numero < 0 && campo.Clave != "descuento_importe")))
                errores.Add($"{campo.Etiqueta}: introduce un número válido sin separadores de miles.");
            if (campo.Clave.StartsWith("fecha_") && !TryFecha(campo.Valor, out _)) errores.Add($"{campo.Etiqueta}: fecha no válida.");
            if (campo.Clave == "cups" && !Coincidir(campo.Valor.Trim(), @"^ES\d{16}[A-Z]{2}(?:[A-Z0-9]{2})?$").Success)
                errores.Add("El CUPS no tiene un formato válido. Corrígelo o desmarca ese campo.");
        }
        var suministro = factura.Obtener("tipo_suministro");
        if (!string.IsNullOrEmpty(suministro) && suministro != "Luz" && suministro != "Gas")
            errores.Add("Selecciona Luz o Gas; una factura mixta requiere separar el importe y el consumo de cada suministro.");
        if (TryFecha(factura.Obtener("fecha_inicio"), out var inicio) && TryFecha(factura.Obtener("fecha_fin"), out var fin) && fin <= inicio)
            errores.Add("La fecha de fin debe ser posterior al inicio de facturación.");
        if (seleccionados.Any(campo => campo.Clave.StartsWith("precio_potencia_p")) && factura.Obtener("unidad_potencia") is not ("dia" or "mes" or "ano"))
            errores.Add("Confirma y selecciona la unidad de los precios de potencia: dia, mes o ano.");
        return errores;
    }

    public static void Revisar(FacturaImportada factura)
    {
        foreach (var campo in factura.Campos.Where(campo => campo.Seleccionado && !string.IsNullOrWhiteSpace(campo.Valor)))
        {
            if (EsNumerico(campo.Clave) && !TryNumero(campo.Valor, out _))
                Avisar(factura, $"{campo.Etiqueta}: formato numérico ambiguo; confirma el valor.");
            if (campo.Clave.StartsWith("fecha_") && !TryFecha(campo.Valor, out _))
                Avisar(factura, $"{campo.Etiqueta}: fecha no reconocida.");
        }
        if (TryFecha(factura.Obtener("fecha_inicio"), out var inicio) && TryFecha(factura.Obtener("fecha_fin"), out var fin) && fin <= inicio)
            Avisar(factura, "Las fechas de facturación están invertidas o no abarcan ningún día.");
        if (TryNumero(factura.Obtener("consumo_total"), out var total))
        {
            var consumos = factura.Campos.Where(campo => campo.Clave.StartsWith("consumo_p") && campo.Seleccionado && TryNumero(campo.Valor, out _)).ToList();
            if (consumos.Count > 0 && Math.Abs(consumos.Sum(campo => { TryNumero(campo.Valor, out var consumo); return consumo; }) - total) > 0.1m)
                Avisar(factura, "Los consumos por periodo no suman el consumo total. Revisa que sean de la misma factura.");
            if (consumos.Count == 0) Avisar(factura, "Solo se conoce el consumo total; no se repartirá entre periodos eléctricos automáticamente.");
        }
        if (string.Equals(factura.Obtener("tipo_suministro"), "Mixto", StringComparison.OrdinalIgnoreCase))
        {
            Avisar(factura, "Factura de luz y gas: separa los importes y consumos de cada suministro antes de comparar.");
            var campoTotal = factura.Campos.First(campo => campo.Clave == "total_factura");
            campoTotal.Seleccionado = false;
            factura.Campos.First(campo => campo.Clave == "consumo_total").Seleccionado = false;
        }
        foreach (var clave in new[] { "fecha_inicio", "fecha_fin", "total_factura" })
            if (string.IsNullOrWhiteSpace(factura.Obtener(clave))) Avisar(factura, $"No se ha identificado {factura.Campos.First(campo => campo.Clave == clave).Etiqueta.ToLowerInvariant()}.");
    }

    public static bool EsNumerico(string clave) => clave != "cups" && !clave.StartsWith("fecha_") && clave != "tipo_suministro" &&
        clave != "peaje_luz" && clave != "peaje_gas" && clave != "unidad_potencia" && clave != "comercializadora_actual" &&
        clave != "nombre_cliente" && clave != "email_cliente" && clave != "direccion_cliente";

    private static FacturaImportada CrearResultado()
    {
        var definiciones = new List<(string, string)>
        {
            ("tipo_suministro", "Suministro: Luz, Gas o Mixto"), ("comercializadora_actual", "Comercializadora"), ("cups", "CUPS"),
            ("peaje_luz", "Peaje de luz"), ("peaje_gas", "Peaje de gas"), ("nombre_cliente", "Titular"), ("email_cliente", "Email"), ("direccion_cliente", "Dirección"),
            ("fecha_inicio", "Inicio de facturación"), ("fecha_fin", "Fin de facturación"), ("total_factura", "Total pagado (€)"), ("consumo_total", "Consumo total (kWh)"),
            ("iva_porcentaje", "IVA (%)"), ("impuesto_electricidad_porcentaje", "Impuesto eléctrico (%)"), ("alquiler_contador", "Alquiler contador (€ del periodo)"),
            ("excedentes_kwh", "Excedentes (kWh)"), ("exceso_potencia", "Exceso potencia (€)"), ("energia_reactiva", "Energía reactiva (€)"), ("bono_social", "Bono social (€)"),
            ("otros_conceptos", "Otros conceptos antes de IVA (€)"), ("descuento_importe", "Descuento actual (€)"), ("precio_energia_medio", "Precio medio energía (€/kWh)"),
            ("unidad_potencia", "Unidad precios potencia: dia, mes o ano"), ("termino_fijo_gas", "Término fijo actual de gas"), ("termino_variable_gas", "Término variable actual de gas (€/kWh)"),
            ("impuesto_hidrocarburos_kwh", "Impuesto hidrocarburos (€/kWh)")
        };
        for (var indice = 1; indice <= 6; indice++)
        {
            definiciones.Add(($"potencia_p{indice}", $"Potencia P{indice} (kW)"));
            definiciones.Add(($"consumo_p{indice}", $"Consumo P{indice} (kWh)"));
            definiciones.Add(($"precio_potencia_p{indice}", $"Precio potencia P{indice}"));
            definiciones.Add(($"precio_energia_p{indice}", $"Precio energía P{indice} (€/kWh)"));
        }
        return new FacturaImportada { Campos = definiciones.Select(campo => new CampoFacturaImportada
        {
            Clave = campo.Item1, Etiqueta = campo.Item2,
            Aplicable = campo.Item1 is not ("precio_energia_medio" or "descuento_importe" or "termino_fijo_gas" or "termino_variable_gas")
        }).ToList() };
    }

    private static void Asignar(FacturaImportada factura, string clave, string valor, string evidencia)
    {
        var campo = factura.Campos.First(campo => campo.Clave == clave);
        if (clave.StartsWith("fecha_") && TryFecha(valor, out var fecha)) valor = fecha.ToString("yyyy-MM-dd");
        if (EsNumerico(clave) && TryNumero(valor, out var numero)) valor = numero.ToString(CultureInfo.InvariantCulture);
        if (campo.Valor.Length > 0 && !string.Equals(campo.Valor, valor, StringComparison.OrdinalIgnoreCase))
        {
            Avisar(factura, $"{campo.Etiqueta}: aparecen valores distintos ({campo.Valor} / {valor}); el campo queda sin seleccionar.");
            campo.Seleccionado = false;
            campo.Conflicto = true;
            return;
        }
        campo.Valor = valor;
        campo.Evidencia = evidencia.Length > 180 ? evidencia[..180] : evidencia;
        campo.Seleccionado = campo.Aplicable && !campo.Conflicto;
    }

    private static void ExtraerNumero(FacturaImportada factura, string linea, string clave, string etiqueta, string unidad)
    {
        if (clave.StartsWith("consumo_") && Coincidir(linea, @"\blecturas?\b").Success &&
            !Coincidir(linea, @"\b(?:consumo|energia)\b").Success) return;
        var coincidencia = Coincidir(linea, $@"(?:^|\b){etiqueta}\s*(?<valor>{Numero}){unidad}");
        if (coincidencia.Success) Asignar(factura, clave, coincidencia.Groups["valor"].Value, linea);
    }

    private static void ExtraerFecha(FacturaImportada factura, string linea, string clave, string etiqueta)
    {
        var coincidencia = Coincidir(linea, $@"{etiqueta}\s*[:=]?\s*(?<fecha>{Fecha})");
        if (coincidencia.Success) Asignar(factura, clave, coincidencia.Groups["fecha"].Value, linea);
    }

    private static Match Coincidir(string texto, string patron) => Regex.Match(texto, patron, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private static void Avisar(FacturaImportada factura, string aviso) { if (!factura.Advertencias.Contains(aviso)) factura.Advertencias.Add(aviso); }
    private static string Normalizar(string texto) => string.Concat(texto.Normalize(NormalizationForm.FormD)
        .Where(caracter => CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)).Normalize(NormalizationForm.FormC).Replace('\r', '\n');
}