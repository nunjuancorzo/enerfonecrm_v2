using System.Globalization;
using System.Security.Cryptography;
using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EnerfoneCRM.Services;

public class ContractSigningPdfService
{
    private readonly ConfiguracionService _configuracionService;
    private readonly DbContextProvider _dbContextProvider;

    public ContractSigningPdfService(ConfiguracionService configuracionService, DbContextProvider dbContextProvider)
    {
        _configuracionService = configuracionService;
        _dbContextProvider = dbContextProvider;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerarDocumentoOriginalAsync(Contrato contrato, Cliente? cliente)
    {
        var configuracion = await _configuracionService.ObtenerConfiguracionAsync();
        var datosTarifa = await ObtenerDatosTarifaAsync(contrato);
        return GenerarDocumento(contrato, cliente, configuracion, datosTarifa.tarifaLuz, datosTarifa.logoComercializadora, null);
    }

    public async Task<byte[]> GenerarDocumentoFirmadoAsync(byte[] documentoOriginal, Contrato contrato, Cliente? cliente, byte[] firma, string ipFirma)
    {
        var configuracion = await _configuracionService.ObtenerConfiguracionAsync();
        var datosTarifa = await ObtenerDatosTarifaAsync(contrato);
        return GenerarDocumento(contrato, cliente, configuracion, datosTarifa.tarifaLuz, datosTarifa.logoComercializadora, firma, ipFirma);
    }

    public async Task<byte[]> GenerarDocumentoColaboradorOriginalAsync(Usuario usuario)
    {
        var configuracion = await _configuracionService.ObtenerConfiguracionAsync();
        return GenerarDocumentoColaboradorCompleto(usuario, configuracion, null, null);
    }

    public async Task<byte[]> GenerarDocumentoColaboradorFirmadoAsync(Usuario usuario, byte[] firma, string ipFirma)
    {
        var configuracion = await _configuracionService.ObtenerConfiguracionAsync();
        return GenerarDocumentoColaboradorCompleto(usuario, configuracion, firma, ipFirma);
    }

    private static byte[] GenerarDocumentoColaboradorCompleto(Usuario usuario, ConfiguracionEmpresa? configuracion, byte[]? firma, string? ipFirma)
    {
        var sourcePath = Path.Combine(AppContext.BaseDirectory, "Resources", "contrato_colaboradores.pdf");
        using var document = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify);
        var nombre = Limpiar(string.Join(" ", new[] { usuario.Nombre, usuario.Apellidos }.Where(x => !string.IsNullOrWhiteSpace(x))));
        var nifCif = Limpiar(usuario.NifCif);
        var domicilio = Limpiar(string.Join(", ", new[]
        {
            usuario.Direccion,
            string.Join(" ", new[] { usuario.CodigoPostal, usuario.Localidad }.Where(x => !string.IsNullOrWhiteSpace(x)))
        }.Where(x => !string.IsNullOrWhiteSpace(x))));
        var cargo = Limpiar(usuario.Rol);
        const string fechaContrato = "25 de septiembre de 2026";

        using (var paginaInicial = XGraphics.FromPdfPage(document.Pages[0], XGraphicsPdfPageOptions.Append))
        {
            var fuente = new XFont("Arial", 10, XFontStyle.Regular);
            CubrirCampo(paginaInicial, 65, 115, 470, 32);
            DibujarTexto(paginaInicial, $"En Mérida (Badajoz), a {fechaContrato}.", fuente, 72, 137);

            CubrirCampo(paginaInicial, 65, 394, 470, 156);
            DibujarTexto(paginaInicial, $"DE OTRA PARTE, D./Dª. {nombre}", fuente, 72, 419);
            DibujarTexto(paginaInicial, $"{domicilio}, provisto/a de", fuente, 72, 438);
            DibujarTexto(paginaInicial, $"C.I.F./N.I.F. número {nifCif}; representada en este acto, en su", fuente, 72, 456);
            DibujarTexto(paginaInicial, $"caso, por D./Dª. {nombre}, mayor de edad, con N.I.F. número {nifCif}", fuente, 72, 474);
            DibujarTexto(paginaInicial, $"{cargo}, cargo para el que se halla facultado en virtud de las", fuente, 72, 492);
            DibujarTexto(paginaInicial, "facultades estatutariamente conferidas y vigentes a la fecha de la firma del presente contrato.", fuente, 72, 510);
        }

        using (var paginaFinal = XGraphics.FromPdfPage(document.Pages[20], XGraphicsPdfPageOptions.Append))
        {
            var fuente = new XFont("Arial", 10, XFontStyle.Regular);
            CubrirCampo(paginaFinal, 285, 505, 250, 165);
            CubrirCampo(paginaFinal, 65, 470, 470, 32);
            DibujarTexto(paginaFinal, $"En Mérida (Badajoz), a {fechaContrato}.", fuente, 72, 494);
        }

        using (var paginaDecimonovena = XGraphics.FromPdfPage(document.Pages[18], XGraphicsPdfPageOptions.Append))
        {
            CubrirCampo(paginaDecimonovena, 285, 125, 250, 175);
        }

        if (firma is { Length: > 0 })
        {
            foreach (var pagina in document.Pages)
            {
                using var paginaConFirma = XGraphics.FromPdfPage(pagina, XGraphicsPdfPageOptions.Append);
                using var imagenFirma = XImage.FromStream(() => new MemoryStream(firma));
                var fondoFirma = new XSolidBrush(XColor.FromArgb(240, 230, 255));
                var fuenteFirma = new XFont("Arial", 8, XFontStyle.Bold);
                var fuenteIp = new XFont("Arial", 7, XFontStyle.Bold);
                paginaConFirma.DrawRectangle(fondoFirma, 42, 784, 511, 48);
                paginaConFirma.DrawString($"Firmado por: {nombre}", fuenteFirma, XBrushes.Black, new XPoint(55, 799));
                paginaConFirma.DrawString($"IP de firma: {ipFirma ?? "-"}", fuenteIp, XBrushes.Black, new XPoint(55, 813));
                paginaConFirma.DrawString("Documento de colaborador", fuenteIp, XBrushes.Black, new XPoint(55, 826));
                paginaConFirma.DrawImage(imagenFirma, 425, 787, 120, 39);
            }
        }

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static void CubrirCampo(XGraphics graphics, double x, double y, double width, double height) =>
        graphics.DrawRectangle(XBrushes.White, x, y, width, height);

    private static void DibujarTexto(XGraphics graphics, string texto, XFont fuente, double x, double y) =>
        graphics.DrawString(texto, fuente, XBrushes.Black, new XPoint(x, y));

    private static string LimpiarPaginaExtraida(string pagina)
    {
        var lineas = pagina.Replace("\r", string.Empty).Split('\n');
        var parrafos = new List<string>();
        var actual = new List<string>();

        void AgregarParrafo()
        {
            if (actual.Count == 0) return;
            parrafos.Add(string.Join(" ", actual));
            actual.Clear();
        }

        foreach (var linea in lineas)
        {
            var textoLinea = System.Text.RegularExpressions.Regex.Replace(linea.Trim(), @"\s+", " ");
            if (string.IsNullOrWhiteSpace(textoLinea))
            {
                AgregarParrafo();
                continue;
            }

            if (textoLinea.StartsWith("Contrato de Agencia y Colaboración Mercantil", StringComparison.OrdinalIgnoreCase) ||
                System.Text.RegularExpressions.Regex.IsMatch(textoLinea, @"^Página \d+ de \d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                continue;
            }

            var esEncabezado = textoLinea.Length <= 90 &&
                (textoLinea == textoLinea.ToUpperInvariant() ||
                 System.Text.RegularExpressions.Regex.IsMatch(textoLinea, @"^\d+(\.\d+)*\.\s"));
            if (esEncabezado) AgregarParrafo();

            actual.Add(textoLinea);

            if (esEncabezado) AgregarParrafo();
        }

        AgregarParrafo();
        return string.Join("\n", parrafos).Trim();
    }

    private static byte[] GenerarDocumentoColaborador(Usuario usuario, ConfiguracionEmpresa? configuracion, byte[]? firma, string? ipFirma)
    {
        var logo = ObtenerImagen(configuracion?.LogoUrl);
        var nombre = Limpiar(string.Join(" ", new[] { usuario.Nombre, usuario.Apellidos }.Where(x => !string.IsNullOrWhiteSpace(x))));
        var empresa = configuracion?.NombreEmpresa ?? "ENERGIA Y TELEFONIA MERIDA S.L.";
        var direccion = configuracion?.Direccion ?? "Calle Almendralejo, 43, Local 5, 06800 Mérida (Badajoz)";
        var fecha = firma == null ? "Fecha pendiente de firma" : DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(45);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9).FontColor(Colors.Grey.Darken3));
                page.Header().Column(header =>
                {
                    if (logo != null) header.Item().Height(55).Image(logo).FitArea();
                    header.Item().PaddingTop(10).Text("CONTRATO DE AGENCIA Y COLABORACION MERCANTIL").Bold().FontSize(14).FontColor(Colors.Blue.Darken4);
                    header.Item().Text("(Ley 12/1992, de 27 de mayo, sobre Contrato de Agencia)").FontSize(9);
                    header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });
                page.Content().Column(content =>
                {
                    content.Spacing(9);
                    content.Item().Text($"En Mérida (Badajoz), a {fecha}.").Bold();
                    content.Item().PaddingTop(8).Text("REUNIDOS").Bold().FontSize(12).FontColor(Colors.Blue.Darken4);
                    content.Item().Text($"De una parte, {empresa}, con domicilio social en {direccion}, representada por su administrador único.");
                    content.Item().Text($"De otra parte, D./Dña. {nombre}, con domicilio profesional en {usuario.Direccion ?? ""}, email {usuario.Email}, teléfono {usuario.Telefono ?? ""}, en adelante, el Colaborador.");
                    content.Item().Text("Ambas partes se reconocen capacidad legal suficiente para obligarse y formalizar el presente contrato de agencia y colaboración mercantil.");
                    content.Item().PaddingTop(8).Text("MANIFIESTAN").Bold().FontSize(12).FontColor(Colors.Blue.Darken4);
                    content.Item().Text("I.- Que la Mercantil desarrolla actividades de comercialización, intermediación y distribución de servicios de energía, telecomunicaciones y seguridad.");
                    content.Item().Text("II.- Que el Colaborador está interesado en promover dichos productos y servicios con autonomía, organización propia y respeto a la normativa aplicable.");
                    content.Item().Text("III.- Que ambas partes desean regular su relación profesional mediante el presente contrato.");
                    content.Item().PaddingTop(8).Text("CLÁUSULAS").Bold().FontSize(12).FontColor(Colors.Blue.Darken4);
                    for (var i = 1; i <= 12; i++)
                    {
                        content.Item().Text(text =>
                        {
                            text.Span($"{i}. ").Bold();
                            text.Span(i switch
                            {
                                1 => "Objeto y alcance de la colaboración: el Colaborador promoverá los productos y servicios autorizados por la Mercantil.",
                                2 => "Independencia: el Colaborador actuará con autonomía y no existirá relación laboral entre las partes.",
                                3 => "Obligaciones del Colaborador: informar correctamente, proteger los datos y cumplir las instrucciones comerciales.",
                                4 => "Obligaciones de la Mercantil: facilitar información, materiales y liquidar las comisiones que correspondan.",
                                5 => "Comisiones: se calcularán conforme a las condiciones comerciales vigentes y a los contratos efectivamente validados.",
                                6 => "Confidencialidad: la información comercial y de clientes será tratada de forma confidencial.",
                                7 => "Protección de datos: ambas partes cumplirán el RGPD y la normativa española aplicable.",
                                8 => "Duración: el contrato tendrá la duración y prórrogas que acuerden las partes.",
                                9 => "Resolución: podrá resolverse por incumplimiento, preaviso o cualquiera de las causas legalmente previstas.",
                                10 => "Responsabilidad: cada parte responderá de sus propios actos y obligaciones.",
                                11 => "Comunicaciones: se realizarán mediante los datos de contacto facilitados por las partes.",
                                _ => "Jurisdicción: las partes se someten a los juzgados y tribunales que resulten competentes conforme a la ley."
                            });
                        });
                    }
                    content.Item().PaddingTop(10).Text("Y en prueba de conformidad, las partes aceptan el presente contrato.").Bold();
                });
                page.Footer().PaddingTop(8).Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    if (firma != null)
                    {
                        footer.Item().ExtendHorizontal().Background("#F0E6FF").PaddingVertical(7).PaddingHorizontal(42).Row(row =>
                        {
                            row.RelativeItem().Column(column => { column.Item().Text($"Firmado por: {nombre}").FontSize(8).Bold(); column.Item().Text($"IP de firma: {ipFirma}").FontSize(7).Bold(); });
                            row.ConstantItem(155).Height(68).Image(firma).FitArea();
                        });
                    }
                    footer.Item().PaddingTop(5).AlignCenter().Text($"{empresa} | {direccion}").FontSize(7);
                });
            });
        }).GeneratePdf();
    }

    private static byte[] GenerarDocumento(Contrato contrato, Cliente? cliente, ConfiguracionEmpresa? configuracion, TarifaLuz? tarifaLuz, byte[]? logoComercializadora, byte[]? firma, string? ipFirma = null)
    {
        var logo = ObtenerImagen(configuracion?.LogoUrl);
        var nombre = ObtenerNombreCompleto(contrato, cliente);
        var dni = Limpiar(cliente?.DniCif ?? contrato.Dni);
        var telefono = Limpiar(cliente?.Telefono ?? contrato.Telefono);
        var email = Limpiar(cliente?.Email);
        var proveedor = ObtenerProveedor(contrato);
        var referencia = contrato.Id.ToString(CultureInfo.InvariantCulture);
        var empresa = configuracion?.NombreEmpresa ?? "ENERGIA Y TELEFONIA MERIDA S.L.";
        var direccion = configuracion?.Direccion ?? "C/ Almendralejo, 43 local 5";
        var cpCiudad = string.Join(", ", new[] { configuracion?.CodigoPostal, configuracion?.Ciudad }.Where(x => !string.IsNullOrWhiteSpace(x)));
        var emailEmpresa = configuracion?.Email ?? "crm@enerfone.com";
        var web = configuracion?.Web ?? "www.enerfone.com";

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(42);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9).FontColor(Colors.Grey.Darken3));
                page.Header().Column(header =>
                {
                    if (logo != null) header.Item().Height(62).Image(logo).FitArea();
                    else header.Item().Text(empresa).Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
                    header.Item().PaddingTop(12).Text("Acreditacion Venta Presencial").Bold().FontSize(15).FontColor(Colors.Blue.Darken4);
                    header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });
                page.Content().Column(content =>
                {
                    content.Spacing(8);
                    content.Item().Text("EN_AVP_01").FontSize(8).FontColor(Colors.Grey.Darken1);
                    content.Item().Text(text =>
                    {
                        text.Span("Yo, D./Dna. ").Bold(); text.Span(nombre).Underline(); text.Span(" con DNI ").Bold(); text.Span(dni).Underline();
                        text.Span(", telefono ").Bold(); text.Span(telefono).Underline(); text.Span(" y email de contacto ").Bold(); text.Span(email).Underline();
                        text.Span(" (en adelante, el cliente) declaro que he recibido informacion comercial sobre los servicios de suministro de electricidad y/o gas natural, telefonia o alarmas presencialmente, y autorizo expresamente a que la tramitacion y formalizacion del contrato pueda completarse posteriormente de forma digital.");
                    });
                    content.Item().Text($"Asimismo, reconozco haber sido informado de que la contratacion del suministro energetico se realizara con {proveedor} y de que recibire la informacion precontractual correspondiente antes de la formalizacion del contrato.");
                    content.Item().Text("El presente consentimiento se otorga unicamente para permitir la continuacion del proceso de contratacion iniciado presencialmente.");
                    content.Item().PaddingTop(8).Text("INFORMACION SOBRE PROTECCION DE DATOS").Bold();
                    content.Item().Text(text =>
                    {
                        text.Span("1) Responsable del tratamiento de sus datos: ").Bold();
                        text.Span($"sus datos seran tratados por {empresa} (en adelante, Enerfone), con domicilio social en {direccion}{(string.IsNullOrWhiteSpace(cpCiudad) ? "" : ", " + cpCiudad)}; y direccion email de contacto {emailEmpresa}.");
                    });
                    content.Item().Text(text =>
                    {
                        text.Span("2) Finalidad del tratamiento de sus datos y legitimacion: ").Bold();
                        text.Span($"sus datos personales seran tratados para realizar, en su nombre, el alta, modificacion, renovacion y cualquier cuestion de indole contractual relacionada con la contratacion de productos y servicios con {empresa}, si Usted consiente expresamente el tratamiento mediante la cumplimentacion del presente formulario.");
                    });
                    content.Item().Text(text =>
                    {
                        text.Span("3) Plazo de conservacion de los datos: ").Bold();
                        text.Span("los datos personales se trataran hasta que la finalidad para la cual fueron recogidos no sea aplicable o hasta que Usted retire su consentimiento, momento a partir del cual se conservaran debidamente bloqueados durante el plazo legal de prescripcion.");
                    });
                    content.Item().Text(text =>
                    {
                        text.Span("4) Derechos en materia de proteccion de datos: ").Bold();
                        text.Span("Usted podra revocar esta autorizacion y ejercitar sus derechos de acceso, rectificacion, supresion, limitacion, oposicion y portabilidad mediante comunicacion escrita, adjuntando copia de su documento oficial de identificacion.");
                    });
                    content.Item().PaddingTop(10).Border(1).BorderColor(Colors.Blue.Darken2).Background(Colors.Blue.Lighten5).Padding(10).Column(box =>
                    {
                        box.Item().Text("INFORMACION DEL CONTRATO").Bold().FontSize(11).FontColor(Colors.Blue.Darken4);
                        box.Item().PaddingTop(7).Row(row =>
                        {
                            if (logoComercializadora != null && logoComercializadora.Length > 0)
                                row.ConstantItem(82).Height(48).PaddingRight(10).Image(logoComercializadora).FitArea();

                            row.RelativeItem().Column(details =>
                            {
                                details.Item().Text(text =>
                                {
                                    text.Span("REFERENCIA DE CONTRATO: ").Bold();
                                    text.Span(referencia);
                                });
                                details.Item().PaddingTop(4).Text(text =>
                                {
                                    text.Span("CUPS LUZ Y/O GAS: ").Bold();
                                    text.Span(ObtenerCupsContrato(contrato));
                                });
                                details.Item().PaddingTop(4).Text(text =>
                                {
                                    text.Span("COMERCIALIZADORA: ").Bold();
                                    text.Span(ObtenerProveedor(contrato));
                                });
                                details.Item().PaddingTop(4).Text(text =>
                                {
                                    text.Span("TARIFA ELEGIDA: ").Bold();
                                    text.Span(ObtenerTarifaElegida(contrato));
                                });
                                details.Item().PaddingTop(4).Text(text =>
                                {
                                    text.Span("PRECIO DE TARIFA: ").Bold();
                                    text.Span(ObtenerPrecioTarifa(tarifaLuz));
                                });
                            });
                        });
                    });
                    var fechaFirma = firma == null
                        ? "Fecha pendiente de firma"
                        : DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));
                    content.Item().PaddingTop(12).Text(fechaFirma);
                });
                page.Footer().PaddingTop(8).Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    if (firma != null)
                    {
                        footer.Item().ExtendHorizontal().Background("#F0E6FF").PaddingVertical(7).PaddingHorizontal(42).Row(row =>
                        {
                            row.RelativeItem().Column(column => { column.Item().Text($"Firmado por: {nombre}").FontSize(8).Bold(); column.Item().Text($"IP de firma: {ipFirma ?? "-"}").FontSize(7).Bold(); column.Item().Text($"Documento {referencia}").FontSize(7); });
                            row.ConstantItem(130).Height(52).Image(firma).FitArea();
                        });
                    }
                    footer.Item().PaddingTop(5).AlignCenter().Text($"{empresa} | {direccion}{(string.IsNullOrWhiteSpace(cpCiudad) ? "" : ", " + cpCiudad)} | {web}").FontSize(7);
                });
            });
        }).GeneratePdf();
    }

    private static byte[]? ObtenerImagen(string? dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl)) return null;
        var separator = dataUrl.IndexOf(',');
        var base64 = separator >= 0 ? dataUrl[(separator + 1)..] : dataUrl;
        try { return Convert.FromBase64String(base64); } catch (FormatException) { return null; }
    }

    private async Task<(TarifaLuz? tarifaLuz, byte[]? logoComercializadora)> ObtenerDatosTarifaAsync(Contrato contrato)
    {
        await using var context = _dbContextProvider.CreateDbContext();

        TarifaLuz? tarifaLuz = null;
        if (contrato.EnTarifaId.HasValue)
            tarifaLuz = await context.TarifasLuz.AsNoTracking().FirstOrDefaultAsync(t => t.Id == contrato.EnTarifaId.Value);

        if (tarifaLuz == null && !string.IsNullOrWhiteSpace(contrato.EnTarifa) && !string.IsNullOrWhiteSpace(contrato.EnComercializadora))
        {
            tarifaLuz = await context.TarifasLuz.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Nombre == contrato.EnTarifa && t.Empresa == contrato.EnComercializadora);
        }

        var logoComercializadora = string.IsNullOrWhiteSpace(contrato.EnComercializadora)
            ? null
            : await context.Comercializadoras.AsNoTracking()
                .Where(c => c.Nombre == contrato.EnComercializadora)
                .Select(c => c.LogoContenido)
                .FirstOrDefaultAsync();

        return (tarifaLuz, logoComercializadora);
    }

    private static string ObtenerCupsContrato(Contrato contrato)
    {
        var cups = new[]
        {
            string.IsNullOrWhiteSpace(contrato.EnCups) ? null : $"Luz: {contrato.EnCups}",
            string.IsNullOrWhiteSpace(contrato.EnCupsGas) ? null : $"Gas: {contrato.EnCupsGas}"
        };
        return Limpiar(string.Join(" | ", cups.Where(c => c != null)));
    }

    private static string ObtenerTarifaElegida(Contrato contrato)
    {
        var tarifas = new[]
        {
            string.IsNullOrWhiteSpace(contrato.EnTarifa) ? null : $"Luz: {contrato.EnTarifa}",
            string.IsNullOrWhiteSpace(contrato.EnTarifaGas) ? null : $"Gas: {contrato.EnTarifaGas}"
        };
        return Limpiar(string.Join(" | ", tarifas.Where(t => t != null)));
    }

    private static string ObtenerPrecioTarifa(TarifaLuz? tarifa)
    {
        if (tarifa == null) return "No disponible";

        var precios = new[]
        {
            ("P1", tarifa.Potencia1), ("P2", tarifa.Potencia2), ("P3", tarifa.Potencia3),
            ("P4", tarifa.Potencia4), ("P5", tarifa.Potencia5), ("P6", tarifa.Potencia6),
            ("E1", tarifa.Energia1), ("E2", tarifa.Energia2), ("E3", tarifa.Energia3),
            ("E4", tarifa.Energia4), ("E5", tarifa.Energia5), ("E6", tarifa.Energia6)
        };
        var valores = precios
            .Where(p => !string.IsNullOrWhiteSpace(p.Item2))
            .Select(p => $"{p.Item1}: {p.Item2}");
        return Limpiar(string.Join(" | ", valores));
    }

    private static string ObtenerNombreCompleto(Contrato contrato, Cliente? cliente)
    {
        var nombre = cliente?.Nombre ?? contrato.NombreCliente;
        var apellidos = cliente?.Apellidos;
        return Limpiar(string.Join(" ", new[] { nombre, apellidos }.Where(s => !string.IsNullOrWhiteSpace(s))));
    }

    private static string ObtenerProveedor(Contrato contrato)
    {
        if (string.Equals(contrato.Tipo, "Telefonia", StringComparison.OrdinalIgnoreCase) || string.Equals(contrato.Tipo, "Telefonía", StringComparison.OrdinalIgnoreCase)) return Limpiar(contrato.OperadoraTel);
        if (string.Equals(contrato.Tipo, "Alarmas", StringComparison.OrdinalIgnoreCase)) return Limpiar(contrato.EmpresaAlarma);
        return Limpiar(contrato.EnComercializadora);
    }

    private static string Limpiar(string? value) => string.IsNullOrWhiteSpace(value) ? "" : value.Replace('\r', ' ').Replace('\n', ' ').Trim();
    public static string CalcularHash(byte[] contenido) => Convert.ToHexString(SHA256.HashData(contenido));
}
