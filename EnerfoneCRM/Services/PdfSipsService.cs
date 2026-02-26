using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EnerfoneCRM.Models;

namespace EnerfoneCRM.Services;

public class PdfSipsService
{
    public byte[] GenerarInformeSips(ClienteSips cliente, List<ConsumoSips> consumos)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var fechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var consumosOrdenados = consumos.OrderByDescending(c => c.FechaFin).ToList();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header()
                    .PaddingBottom(10)
                    .Column(column =>
                    {
                        column.Item().Text("ANÁLISIS SIPS - INFORME DE SUMINISTRO")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Purple.Darken2)
                            .AlignCenter();

                        column.Item().PaddingTop(5).Text($"Fecha de emisión: {fechaActual}")
                            .FontSize(9)
                            .AlignCenter()
                            .FontColor(Colors.Grey.Darken1);

                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Purple.Darken2);
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        // Información del Suministro
                        column.Item()
                            .Background(Colors.Purple.Lighten4)
                            .Padding(10)
                            .Column(infoColumn =>
                            {
                                infoColumn.Item().PaddingBottom(8).Text("INFORMACIÓN DEL SUMINISTRO")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Purple.Darken2);

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("CUPS: ").Bold().FontSize(9);
                                        txt.Span(cliente.CodigoCUPS ?? "").FontSize(9);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Distribuidora: ").Bold().FontSize(9);
                                        txt.Span(cliente.NombreEmpresaDistribuidora ?? "").FontSize(9);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem(1).Text(txt =>
                                    {
                                        txt.Span("Provincia: ").Bold().FontSize(9);
                                        txt.Span(cliente.DesProvinciaPS ?? "").FontSize(9);
                                    });
                                    
                                    row.RelativeItem(1).Text(txt =>
                                    {
                                        txt.Span("Municipio: ").Bold().FontSize(9);
                                        txt.Span(cliente.DesMunicipioPS ?? "").FontSize(9);
                                    });
                                    
                                    row.RelativeItem(1).Text(txt =>
                                    {
                                        txt.Span("C.P.: ").Bold().FontSize(9);
                                        txt.Span(cliente.CodigoPostalPS ?? "").FontSize(9);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem(1).Text(txt =>
                                    {
                                        txt.Span("Tarifa ATR: ").Bold().FontSize(9);
                                        txt.Span(cliente.CodigoTarifaATREnVigor ?? "").FontSize(9);
                                    });
                                    
                                    row.RelativeItem(1).Text(txt =>
                                    {
                                        txt.Span("Perfil: ").Bold().FontSize(9);
                                        txt.Span(cliente.TipoPerfilConsumo ?? "").FontSize(9);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Fecha Alta: ").Bold().FontSize(9);
                                        txt.Span(cliente.FechaAltaSuministro?.ToString("dd/MM/yyyy") ?? "").FontSize(9);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Última Lectura: ").Bold().FontSize(9);
                                        txt.Span(cliente.FechaUltimaLectura?.ToString("dd/MM/yyyy") ?? "").FontSize(9);
                                    });
                                });
                            });

                        column.Item().PaddingTop(15);

                        // Potencias Contratadas
                        column.Item()
                            .Background(Colors.Blue.Lighten4)
                            .Padding(10)
                            .Column(potColumn =>
                            {
                                potColumn.Item().PaddingBottom(8).Text("POTENCIAS CONTRATADAS")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                potColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("P1: ").Bold().FontSize(9);
                                        txt.Span($"{cliente.PotenciasContratadasEnWP1:N2} kW").FontSize(9);
                                    });
                                    
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("P2: ").Bold().FontSize(9);
                                        txt.Span($"{cliente.PotenciasContratadasEnWP2:N2} kW").FontSize(9);
                                    });
                                    
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("P3: ").Bold().FontSize(9);
                                        txt.Span($"{cliente.PotenciasContratadasEnWP3:N2} kW").FontSize(9);
                                    });
                                });
                            });

                        column.Item().PaddingTop(15);

                        // Estadísticas de Consumo
                        var consumoPromedio = consumos.Average(c => c.TotalConsumo);
                        var consumoMaximo = consumos.Max(c => c.TotalConsumo);
                        var consumoMinimo = consumos.Min(c => c.TotalConsumo);
                        var consumoTotal = consumos.Sum(c => c.TotalConsumo);

                        column.Item()
                            .Background(Colors.Green.Lighten4)
                            .Padding(10)
                            .Column(statsColumn =>
                            {
                                statsColumn.Item().PaddingBottom(8).Text("ESTADÍSTICAS DE CONSUMO")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Green.Darken2);

                                statsColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Promedio Mensual: ").Bold().FontSize(9);
                                        txt.Span($"{consumoPromedio:N0} kWh").FontSize(9);
                                    });
                                    
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Máximo Mensual: ").Bold().FontSize(9);
                                        txt.Span($"{consumoMaximo:N0} kWh").FontSize(9);
                                    });
                                });

                                statsColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Mínimo Mensual: ").Bold().FontSize(9);
                                        txt.Span($"{consumoMinimo:N0} kWh").FontSize(9);
                                    });
                                    
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Total Acumulado: ").Bold().FontSize(9);
                                        txt.Span($"{consumoTotal:N0} kWh").FontSize(9);
                                    });
                                });

                                statsColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Registros Analizados: ").Bold().FontSize(9);
                                        txt.Span($"{consumos.Count} meses").FontSize(9);
                                    });
                                });
                            });

                        column.Item().PaddingTop(15);

                        // Distribución por Periodos
                        var totalP1 = consumos.Sum(c => c.Activa1 ?? 0);
                        var totalP2 = consumos.Sum(c => c.Activa2 ?? 0);
                        var totalP3 = consumos.Sum(c => c.Activa3 ?? 0);
                        var totalGeneral = totalP1 + totalP2 + totalP3;

                        column.Item()
                            .Background(Colors.Orange.Lighten4)
                            .Padding(10)
                            .Column(distColumn =>
                            {
                                distColumn.Item().PaddingBottom(8).Text("DISTRIBUCIÓN POR PERIODOS (Acumulado)")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Orange.Darken2);

                                distColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Periodo 1: ").Bold().FontSize(9);
                                        txt.Span($"{totalP1:N0} kWh ({(totalP1 / totalGeneral * 100):N1}%)").FontSize(9);
                                    });
                                });

                                distColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Periodo 2: ").Bold().FontSize(9);
                                        txt.Span($"{totalP2:N0} kWh ({(totalP2 / totalGeneral * 100):N1}%)").FontSize(9);
                                    });
                                });

                                distColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Periodo 3: ").Bold().FontSize(9);
                                        txt.Span($"{totalP3:N0} kWh ({(totalP3 / totalGeneral * 100):N1}%)").FontSize(9);
                                    });
                                });

                                // Gráfico de barras para distribución por periodos
                                distColumn.Item().PaddingTop(10).Height(120).Row(row =>
                                {
                                    var maxValue = Math.Max(Math.Max(totalP1, totalP2), totalP3);
                                    var heightP1 = maxValue > 0 ? (int)(totalP1 / maxValue * 100) : 0;
                                    var heightP2 = maxValue > 0 ? (int)(totalP2 / maxValue * 100) : 0;
                                    var heightP3 = maxValue > 0 ? (int)(totalP3 / maxValue * 100) : 0;

                                    // Periodo 1
                                    row.RelativeItem().PaddingHorizontal(5).Column(col =>
                                    {
                                        col.Item().Height(100 - heightP1).AlignBottom();
                                        col.Item().Height(heightP1).Background(Colors.Blue.Medium);
                                        col.Item().PaddingTop(5).AlignCenter().Text("P1").FontSize(8).Bold().FontColor(Colors.Blue.Darken2);
                                    });

                                    // Periodo 2
                                    row.RelativeItem().PaddingHorizontal(5).Column(col =>
                                    {
                                        col.Item().Height(100 - heightP2).AlignBottom();
                                        col.Item().Height(heightP2).Background(Colors.Green.Medium);
                                        col.Item().PaddingTop(5).AlignCenter().Text("P2").FontSize(8).Bold().FontColor(Colors.Green.Darken2);
                                    });

                                    // Periodo 3
                                    row.RelativeItem().PaddingHorizontal(5).Column(col =>
                                    {
                                        col.Item().Height(100 - heightP3).AlignBottom();
                                        col.Item().Height(heightP3).Background(Colors.Orange.Medium);
                                        col.Item().PaddingTop(5).AlignCenter().Text("P3").FontSize(8).Bold().FontColor(Colors.Orange.Darken2);
                                    });
                                });
                            });

                        column.Item().PaddingTop(15);

                        // Gráfico de evolución mensual
                        var ultimos12 = consumosOrdenados.Take(12).Reverse().ToList();
                        if (ultimos12.Any())
                        {
                            column.Item()
                                .Background(Colors.Blue.Lighten4)
                                .Padding(10)
                                .Column(graficoColumn =>
                                {
                                    graficoColumn.Item().PaddingBottom(8).Text("EVOLUCIÓN DEL CONSUMO (Últimos 12 meses)")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.Blue.Darken2);

                                    graficoColumn.Item().Height(110).Row(row =>
                                    {
                                        var maxConsumo = ultimos12.Max(c => c.TotalConsumo);

                                        foreach (var consumo in ultimos12)
                                        {
                                            var heightPercentage = maxConsumo > 0 ? (int)(consumo.TotalConsumo / maxConsumo * 90) : 0;
                                            
                                            row.RelativeItem().PaddingHorizontal(2).Column(col =>
                                            {
                                                col.Item().Height(90 - heightPercentage);
                                                col.Item().Height(heightPercentage).Background(Colors.Blue.Medium).AlignBottom();
                                                col.Item().PaddingTop(3).AlignCenter().Text(consumo.FechaInicio?.ToString("MMM") ?? "").FontSize(6).FontColor(Colors.Grey.Darken1);
                                            });
                                        }
                                    });

                                    graficoColumn.Item().PaddingTop(5).Text("Cada barra representa el consumo total mensual en kWh").FontSize(7).FontColor(Colors.Grey.Darken1).Italic();
                                });

                            column.Item().PaddingTop(15);
                        }

                        // Histórico de Consumos (Tabla)
                        column.Item().PaddingBottom(5).Text($"HISTÓRICO DE CONSUMOS ({consumos.Count} registros)")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Grey.Darken2);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f); // Periodo
                                columns.RelativeColumn(1.5f); // Fecha Inicio
                                columns.RelativeColumn(1.5f); // Fecha Fin
                                columns.RelativeColumn(1f);   // P1
                                columns.RelativeColumn(1f);   // P2
                                columns.RelativeColumn(1f);   // P3
                                columns.RelativeColumn(1.2f); // Total
                            });

                            // Encabezado
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Darken1).Padding(5).AlignCenter().Text("Periodo").FontColor(Colors.White).Bold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Darken1).Padding(5).AlignCenter().Text("F. Inicio").FontColor(Colors.White).Bold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Darken1).Padding(5).AlignCenter().Text("F. Fin").FontColor(Colors.White).Bold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Darken1).Padding(5).AlignCenter().Text("P1 (kWh)").FontColor(Colors.White).Bold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Darken1).Padding(5).AlignCenter().Text("P2 (kWh)").FontColor(Colors.White).Bold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Darken1).Padding(5).AlignCenter().Text("P3 (kWh)").FontColor(Colors.White).Bold().FontSize(8);
                                header.Cell().Background(Colors.Grey.Darken1).Padding(5).AlignCenter().Text("Total (kWh)").FontColor(Colors.White).Bold().FontSize(8);
                            });

                            // Datos
                            foreach (var c in consumosOrdenados)
                            {
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(txt => txt.Span(c.FechaInicio?.ToString("MMM yyyy") ?? "").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(txt => txt.Span(c.FechaInicio?.ToString("dd/MM/yy") ?? "").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(txt => txt.Span(c.FechaFin?.ToString("dd/MM/yy") ?? "").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text(txt => txt.Span($"{c.Activa1 ?? 0:N0}").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text(txt => txt.Span($"{c.Activa2 ?? 0:N0}").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text(txt => txt.Span($"{c.Activa3 ?? 0:N0}").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text(txt => txt.Span($"{c.TotalConsumo:N0}").FontSize(7).Bold());
                            }

                            // Totales
                            table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten2).Padding(5).Text(txt => txt.Span("TOTAL ACUMULADO").Bold().FontSize(8));
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(txt => txt.Span($"{totalP1:N0}").Bold().FontSize(8));
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(txt => txt.Span($"{totalP2:N0}").Bold().FontSize(8));
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(txt => txt.Span($"{totalP3:N0}").Bold().FontSize(8));
                            table.Cell().Background(Colors.Green.Lighten2).Padding(5).AlignRight().Text(txt => txt.Span($"{totalGeneral:N0}").Bold().FontSize(8));
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Column(column =>
                    {
                        column.Item()
                            .Background(Colors.Purple.Darken2)
                            .Padding(8)
                            .AlignCenter()
                            .Text(txt => txt.Span("Informe generado por SIPS - Sistema de Integración de Pagos Seguros").FontSize(9).FontColor(Colors.White));

                        column.Item().PaddingTop(5).Text(txt =>
                        {
                            txt.Span("Página ").FontSize(8);
                            txt.CurrentPageNumber().FontSize(8);
                            txt.Span(" de ").FontSize(8);
                            txt.TotalPages().FontSize(8);
                        });
                    });
            });
        });

        return documento.GeneratePdf();
    }
}
