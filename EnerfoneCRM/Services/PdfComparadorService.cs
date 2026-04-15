using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EnerfoneCRM.Services;

public class PdfComparadorService
{
    public class DatosComparativa
    {
        public string Cups { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string DireccionCliente { get; set; } = string.Empty;
        public string CompaniaActual { get; set; } = string.Empty;
        public string TarifaActual { get; set; } = string.Empty;
        public decimal ConsumoAnual { get; set; }
        public decimal PotenciaPunta { get; set; }
        public decimal PotenciaValle { get; set; }
        public byte[]? LogoCompaniaActual { get; set; }
        public List<OfertaComparativa> Ofertas { get; set; } = new();
    }

    public class OfertaComparativa
    {
        public int Posicion { get; set; }
        public string Empresa { get; set; } = string.Empty;
        public string NombreTarifa { get; set; } = string.Empty;
        public decimal CosteTerminoFijo { get; set; }
        public decimal CostePotencia { get; set; }
        public decimal CosteEnergia { get; set; }
        public decimal CosteMensual { get; set; }
        public decimal CosteActual { get; set; }
        public decimal AhorroMensual { get; set; }
        public decimal AhorroAnual { get; set; }
        public decimal PorcentajeAhorro { get; set; }
        public byte[]? LogoEmpresa { get; set; }
    }

    public byte[] GenerarComparativaPdf(DatosComparativa datos)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var fechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // CABECERA
                page.Header()
                    .PaddingBottom(15)
                    .Column(column =>
                    {
                        column.Item()
                            .Background(Colors.Green.Darken2)
                            .Padding(20)
                            .Column(col =>
                            {
                                col.Item().Text("COMPARATIVA DE TARIFAS ENERGÉTICAS")
                                    .FontSize(22)
                                    .Bold()
                                    .FontColor(Colors.White)
                                    .AlignCenter();

                                col.Item().PaddingTop(8).Text($"Generado el {fechaActual}")
                                    .FontSize(10)
                                    .AlignCenter()
                                    .FontColor(Colors.White);
                            });
                    });

                // CONTENIDO
                page.Content()
                    .PaddingVertical(15)
                    .Column(column =>
                    {
                        // Datos del cliente con diseño mejorado
                        column.Item().PaddingBottom(25).Column(col =>
                        {
                            col.Item().Background(Colors.Green.Darken2).Padding(10).Text("INFORMACIÓN DEL CLIENTE")
                                .FontSize(13)
                                .Bold()
                                .FontColor(Colors.White);

                            col.Item().Border(1).BorderColor(Colors.Green.Darken2).Padding(20).Background(Colors.Grey.Lighten5).Column(c =>
                            {
                                c.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(innerCol =>
                                    {
                                        innerCol.Item().Text(txt =>
                                        {
                                            txt.Span("👤 ").FontSize(14);
                                            txt.Span("Nombre: ").Bold().FontSize(11);
                                            txt.Span(datos.NombreCliente).FontSize(11);
                                        });
                                        innerCol.Item().PaddingTop(8).Text(txt =>
                                        {
                                            txt.Span("📧 ").FontSize(14);
                                            txt.Span("Email: ").Bold().FontSize(11);
                                            txt.Span(datos.EmailCliente).FontSize(11);
                                        });
                                        innerCol.Item().PaddingTop(8).Text(txt =>
                                        {
                                            txt.Span("📍 ").FontSize(14);
                                            txt.Span("Dirección: ").Bold().FontSize(11);
                                            txt.Span(datos.DireccionCliente).FontSize(11);
                                        });
                                        innerCol.Item().PaddingTop(8).Text(txt =>
                                        {
                                            txt.Span("🔌 ").FontSize(14);
                                            txt.Span("CUPS: ").Bold().FontSize(11);
                                            txt.Span(datos.Cups).FontSize(10).FontColor(Colors.Blue.Darken2);
                                        });
                                    });
                                });
                            });
                        });

                        // Suministro actual con logo
                        column.Item().PaddingBottom(25).Column(col =>
                        {
                            col.Item().Background(Colors.Blue.Darken2).Padding(10).Text("TU SUMINISTRO ACTUAL")
                                .FontSize(13)
                                .Bold()
                                .FontColor(Colors.White);

                            col.Item().Border(1).BorderColor(Colors.Blue.Darken2).Padding(20).Background(Colors.Blue.Lighten5).Row(row =>
                            {
                                // Logo de la comercializadora actual
                                if (datos.LogoCompaniaActual != null && datos.LogoCompaniaActual.Length > 0)
                                {
                                    row.ConstantItem(100).Padding(5).Image(datos.LogoCompaniaActual).FitArea();
                                }

                                row.RelativeItem().PaddingLeft(15).Column(c =>
                                {
                                    c.Item().Text(datos.CompaniaActual)
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(Colors.Blue.Darken3);

                                    c.Item().PaddingTop(10).Text(txt =>
                                    {
                                        txt.Span("Tarifa contratada: ").Bold().FontSize(11);
                                        txt.Span(datos.TarifaActual).FontSize(11).FontColor(Colors.Blue.Darken2);
                                    });

                                    c.Item().PaddingTop(6).Row(innerRow =>
                                    {
                                        innerRow.RelativeItem().Text(txt =>
                                        {
                                            txt.Span("Consumo anual: ").Bold().FontSize(10);
                                            txt.Span($"{datos.ConsumoAnual:N0} kWh").FontSize(10);
                                        });
                                        innerRow.RelativeItem().Text(txt =>
                                        {
                                            txt.Span("Potencia P1: ").Bold().FontSize(10);
                                            txt.Span($"{datos.PotenciaPunta:N2} kW").FontSize(10);
                                        });
                                        innerRow.RelativeItem().Text(txt =>
                                        {
                                            txt.Span("Potencia P2: ").Bold().FontSize(10);
                                            txt.Span($"{datos.PotenciaValle:N2} kW").FontSize(10);
                                        });
                                    });

                                    c.Item().PaddingTop(10).Background(Colors.Blue.Darken2).Padding(8).Text(txt =>
                                    {
                                        txt.Span("Coste actual: ").FontColor(Colors.White).FontSize(11);
                                        txt.Span($"{datos.Ofertas.FirstOrDefault()?.CosteActual ?? 0:N2} €/mes").Bold().FontSize(14).FontColor(Colors.Yellow.Lighten2);
                                    });
                                });
                            });
                        });

                        // Separador visual
                        column.Item().PaddingBottom(15).LineHorizontal(2).LineColor(Colors.Green.Darken2);

                        // Título de ofertas
                        column.Item().PaddingBottom(15).Background(Colors.Green.Darken2).Padding(10).Text($"TUS MEJORES OPCIONES ({datos.Ofertas.Count} {(datos.Ofertas.Count == 1 ? "OFERTA" : "OFERTAS")})")
                            .FontSize(13)
                            .Bold()
                            .FontColor(Colors.White);

                        // Ofertas con diseño mejorado
                        foreach (var oferta in datos.Ofertas)
                        {
                            var esMejor = oferta.Posicion == 1;
                            var colorBorde = esMejor ? Colors.Green.Darken2 : Colors.Grey.Medium;
                            var grosorBorde = esMejor ? 3 : 1;

                            column.Item().PaddingBottom(20).Border(grosorBorde).BorderColor(colorBorde).Column(col =>
                            {
                                // Cabecera de la oferta con badge
                                col.Item().Background(esMejor ? Colors.Green.Darken2 : Colors.Grey.Darken1).Padding(12).Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span($"OPCIÓN #{oferta.Posicion}  ").Bold().FontSize(14).FontColor(Colors.White);
                                        if (esMejor)
                                        {
                                            txt.Span("⭐ MEJOR OFERTA ⭐").Bold().FontSize(12).FontColor(Colors.Yellow.Lighten2);
                                        }
                                    });

                                    row.AutoItem().AlignRight().Text($"Ahorro anual: {oferta.AhorroAnual:N2} €")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor(Colors.Yellow.Lighten2);
                                });

                                // Contenido de la oferta
                                col.Item().Padding(20).Background(esMejor ? Colors.Green.Lighten5 : Colors.White).Row(row =>
                                {
                                    // Logo de la nueva comercializadora
                                    if (oferta.LogoEmpresa != null && oferta.LogoEmpresa.Length > 0)
                                    {
                                        row.ConstantItem(120).Padding(10).Column(c =>
                                        {
                                            c.Item().Image(oferta.LogoEmpresa).FitArea();
                                            c.Item().PaddingTop(10).AlignCenter().Text(oferta.Empresa)
                                                .FontSize(9)
                                                .Bold()
                                                .FontColor(Colors.Grey.Darken2);
                                        });
                                    }
                                    else
                                    {
                                        row.ConstantItem(120).Padding(10).Column(c =>
                                        {
                                            c.Item().AlignMiddle().AlignCenter().Text(oferta.Empresa)
                                                .FontSize(12)
                                                .Bold()
                                                .FontColor(Colors.Grey.Darken2);
                                        });
                                    }

                                    row.RelativeItem().PaddingLeft(15).Column(c =>
                                    {
                                        // Nombre de la tarifa
                                        c.Item().Text(oferta.NombreTarifa)
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Green.Darken3);

                                        // Desglose detallado
                                        c.Item().PaddingTop(15).Row(innerRow =>
                                        {
                                            // Columna 1: Costes desglosados
                                            innerRow.RelativeItem().Padding(5).Column(desglose =>
                                            {
                                                desglose.Item().Background(Colors.Grey.Lighten3).Padding(8).Text("DESGLOSE DE COSTES")
                                                    .FontSize(9)
                                                    .Bold()
                                                    .FontColor(Colors.Grey.Darken2);

                                                desglose.Item().PaddingTop(8).Row(r =>
                                                {
                                                    r.RelativeItem().Text("Término fijo:").FontSize(10);
                                                    r.AutoItem().Text($"{oferta.CosteTerminoFijo:N2} €/mes").Bold().FontSize(10);
                                                });

                                                desglose.Item().PaddingTop(5).Row(r =>
                                                {
                                                    r.RelativeItem().Text("Potencia:").FontSize(10);
                                                    r.AutoItem().Text($"{oferta.CostePotencia:N2} €/mes").Bold().FontSize(10);
                                                });

                                                desglose.Item().PaddingTop(5).Row(r =>
                                                {
                                                    r.RelativeItem().Text("Energía:").FontSize(10);
                                                    r.AutoItem().Text($"{oferta.CosteEnergia:N2} €/mes").Bold().FontSize(10);
                                                });

                                                desglose.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                                                desglose.Item().PaddingTop(8).Row(r =>
                                                {
                                                    r.RelativeItem().Text("TOTAL MENSUAL:").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                                                    r.AutoItem().Text($"{oferta.CosteMensual:N2} €").Bold().FontSize(13).FontColor(Colors.Blue.Darken3);
                                                });
                                            });

                                            // Columna 2: Ahorro
                                            innerRow.RelativeItem().PaddingLeft(10).Padding(5).Column(ahorro =>
                                            {
                                                ahorro.Item().Background(Colors.Green.Lighten3).Padding(8).Text("TU AHORRO")
                                                    .FontSize(9)
                                                    .Bold()
                                                    .FontColor(Colors.Green.Darken3);

                                                if (oferta.AhorroMensual > 0)
                                                {
                                                    ahorro.Item().PaddingTop(10).AlignCenter().Text($"{oferta.AhorroMensual:N2} €")
                                                        .FontSize(20)
                                                        .Bold()
                                                        .FontColor(Colors.Green.Darken3);

                                                    ahorro.Item().PaddingTop(2).AlignCenter().Text("al mes")
                                                        .FontSize(9)
                                                        .FontColor(Colors.Green.Darken2);

                                                    ahorro.Item().PaddingTop(10).AlignCenter().Background(Colors.Green.Darken2).Padding(8).Text($"-{oferta.PorcentajeAhorro:N1}%")
                                                        .FontSize(16)
                                                        .Bold()
                                                        .FontColor(Colors.White);

                                                    ahorro.Item().PaddingTop(10).AlignCenter().Text($"{oferta.AhorroAnual:N2} € al año")
                                                        .FontSize(11)
                                                        .Bold()
                                                        .FontColor(Colors.Orange.Darken2);
                                                }
                                                else
                                                {
                                                    ahorro.Item().PaddingTop(20).AlignCenter().Text("Sin ahorro estimado")
                                                        .FontSize(11)
                                                        .FontColor(Colors.Grey.Darken1);
                                                }
                                            });
                                        });

                                        // Comparación visual actual vs nueva
                                        c.Item().PaddingTop(15).Background(Colors.Grey.Lighten4).Padding(10).Row(compRow =>
                                        {
                                            compRow.RelativeItem().Column(actual =>
                                            {
                                                actual.Item().Text("📊 Factura actual")
                                                    .FontSize(9)
                                                    .FontColor(Colors.Grey.Darken2);
                                                actual.Item().PaddingTop(3).Text($"{oferta.CosteActual:N2} €/mes")
                                                    .FontSize(12)
                                                    .Bold()
                                                    .FontColor(Colors.Red.Darken1);
                                            });

                                            compRow.ConstantItem(50).AlignMiddle().AlignCenter().Text("➔")
                                                .FontSize(20)
                                                .FontColor(Colors.Green.Darken2);

                                            compRow.RelativeItem().Column(nueva =>
                                            {
                                                nueva.Item().Text("📈 Nueva factura")
                                                    .FontSize(9)
                                                    .FontColor(Colors.Grey.Darken2);
                                                nueva.Item().PaddingTop(3).Text($"{oferta.CosteMensual:N2} €/mes")
                                                    .FontSize(12)
                                                    .Bold()
                                                    .FontColor(Colors.Green.Darken2);
                                            });
                                        });
                                    });
                                });
                            });
                        }

                        // Nota legal mejorada
                        column.Item().PaddingTop(20).Border(2).BorderColor(Colors.Orange.Darken1).Background(Colors.Orange.Lighten4).Padding(15).Column(legal =>
                        {
                            legal.Item().Text("⚠ INFORMACIÓN IMPORTANTE")
                                .FontSize(11)
                                .Bold()
                                .FontColor(Colors.Orange.Darken3);

                            legal.Item().PaddingTop(8).Text("Los precios mostrados en esta comparativa son estimaciones basadas en las tarifas actuales y los datos de consumo proporcionados. Los costes finales pueden variar según condiciones contractuales, promociones vigentes, impuestos aplicables y otros factores. Esta comparativa tiene carácter informativo y no constituye una oferta vinculante. Se recomienda verificar las condiciones definitivas con cada comercializadora antes de realizar cualquier cambio de contrato.")
                                .FontSize(8)
                                .LineHeight(1.3f)
                                .FontColor(Colors.Grey.Darken2);
                        });
                    });

                // PIE DE PÁGINA mejorado
                page.Footer()
                    .Column(column =>
                    {
                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        
                        column.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().AlignLeft().Text($"Comparativa generada el {fechaActual}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);

                            row.RelativeItem().AlignCenter()
                                .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                                .Text(txt =>
                                {
                                    txt.Span("Página ");
                                    txt.CurrentPageNumber();
                                    txt.Span(" de ");
                                    txt.TotalPages();
                                });

                            row.RelativeItem().AlignRight().Text("Comparador de tarifas energéticas")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });
            });
        });

        return documento.GeneratePdf();
    }
}
