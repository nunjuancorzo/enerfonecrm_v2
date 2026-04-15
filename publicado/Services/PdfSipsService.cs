using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EnerfoneCRM.Models;

namespace EnerfoneCRM.Services;

public class PdfSipsService
{
    public byte[] GenerarInformeSips(ClienteSips cliente, List<ConsumoSips>? consumos = null, byte[]? logoEmpresa = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var fechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var consumosOrdenados = consumos?.OrderByDescending(c => c.FechaFin).ToList() ?? new List<ConsumoSips>();
        var hayConsumos = consumosOrdenados.Any();

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
                        // Logo y título
                        column.Item().Row(row =>
                        {
                            // Logo de la empresa (si existe)
                            if (logoEmpresa != null && logoEmpresa.Length > 0)
                            {
                                row.ConstantItem(100).Height(60).AlignMiddle().Image(logoEmpresa).FitArea();
                            }
                            
                            row.RelativeItem().PaddingLeft(logoEmpresa != null ? 15 : 0).Column(col =>
                            {
                                col.Item().Text("ANÁLISIS SIPS - INFORME DE SUMINISTRO")
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(Colors.Black);

                                col.Item().PaddingTop(5).Text($"Fecha de emisión: {fechaActual}")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });

                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Orange.Darken2);
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        // Información del Suministro
                        column.Item()
                            .Border(1)
                            .BorderColor(Colors.Yellow.Darken1)
                            
                            .Background(Colors.Yellow.Lighten4)
                            .Padding(10)
                            .Column(infoColumn =>
                            {
                                infoColumn.Item().PaddingBottom(8).Text("INFORMACIÓN DEL SUMINISTRO")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Black);

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

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Último Movimiento Contrato: ").Bold().FontSize(9);
                                        txt.Span(cliente.FechaUltimoMovimientoContrato?.ToString("dd/MM/yyyy") ?? "-").FontSize(9);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Último Cambio Comercializador: ").Bold().FontSize(9);
                                        txt.Span(cliente.FechaUltimoCambioComercializador?.ToString("dd/MM/yyyy") ?? "-").FontSize(9);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Bono Social: ").Bold().FontSize(9);
                                        var bonoSocial = (cliente.AplicacionBonoSocial == "0" || string.IsNullOrEmpty(cliente.AplicacionBonoSocial)) 
                                            ? "No aplicado" 
                                            : "Sí aplicado";
                                        txt.Span(bonoSocial).FontSize(9);
                                    });
                                });
                            });

                        column.Item().PaddingTop(10);

                        // Potencias Contratadas
                        column.Item()
                            .Border(1)
                            .BorderColor(Colors.Yellow.Darken2)
                            
                            .Background(Colors.Yellow.Lighten3)
                            .Padding(10)
                            .Column(potColumn =>
                            {
                                potColumn.Item().PaddingBottom(8).Text("POTENCIAS CONTRATADAS")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Black);

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

                        column.Item().PaddingTop(10);

                        // Mostrar secciones de consumo solo si hay datos disponibles
                        if (hayConsumos)
                        {
                            // Calcular estadísticas principales
                            var consumoPromedio = consumosOrdenados.Average(c => c.TotalConsumo);
                            var consumoMaximo = consumosOrdenados.Max(c => c.TotalConsumo);
                            var consumoMinimo = consumosOrdenados.Min(c => c.TotalConsumo);
                            var consumoTotal = consumosOrdenados.Sum(c => c.TotalConsumo);
                            
                            // Calcular consumos por períodos
                            var ultimos12Meses = consumosOrdenados.Take(12).ToList();
                            var ultimos3Meses = consumosOrdenados.Take(3).ToList();
                            var consumoAnual = ultimos12Meses.Count >= 12 ? ultimos12Meses.Sum(c => c.TotalConsumo) : 0;
                            var consumoTrimestral = ultimos3Meses.Count >= 3 ? ultimos3Meses.Sum(c => c.TotalConsumo) : 0;

                        // Estadísticas principales
                        column.Item()
                            .Border(1)
                            .BorderColor(Colors.Orange.Lighten1)
                            
                            .Background(Colors.Yellow.Lighten2)
                            .Padding(10)
                            .Column(statsColumn =>
                            {
                                statsColumn.Item().PaddingBottom(8).Text("RESUMEN DE CONSUMOS")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Black);

                                // Fila 1: Anual y Trimestral
                                statsColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Consumo Anual (12 meses): ").Bold().FontSize(9);
                                        if (ultimos12Meses.Count >= 12)
                                        {
                                            txt.Span($"{consumoAnual:N0} kWh").FontSize(9);
                                        }
                                        else
                                        {
                                            txt.Span($"N/A (solo {ultimos12Meses.Count} meses)").FontSize(8).Italic();
                                        }
                                    });
                                    
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Consumo Trimestral (3 meses): ").Bold().FontSize(9);
                                        if (ultimos3Meses.Count >= 3)
                                        {
                                            txt.Span($"{consumoTrimestral:N0} kWh").FontSize(9);
                                        }
                                        else
                                        {
                                            txt.Span($"N/A (solo {ultimos3Meses.Count} meses)").FontSize(8).Italic();
                                        }
                                    });
                                });

                                // Fila 2: Promedio y Total
                                statsColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Promedio Mensual: ").Bold().FontSize(9);
                                        txt.Span($"{consumoPromedio:N0} kWh").FontSize(9);
                                    });
                                    
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Total Período Completo: ").Bold().FontSize(9);
                                        txt.Span($"{consumoTotal:N0} kWh").FontSize(9);
                                    });
                                });

                                // Fila 3: Máximo y Mínimo
                                statsColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Máximo Mensual: ").Bold().FontSize(9);
                                        txt.Span($"{consumoMaximo:N0} kWh").FontSize(9);
                                    });
                                    
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Mínimo Mensual: ").Bold().FontSize(9);
                                        txt.Span($"{consumoMinimo:N0} kWh").FontSize(9);
                                    });
                                });

                                statsColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Registros Analizados: ").Bold().FontSize(9);
                                        txt.Span($"{consumosOrdenados.Count} meses").FontSize(9);
                                    });
                                });
                            });

                        column.Item().PaddingTop(10);

                        // Distribución por Periodos
                        var totalP1 = consumosOrdenados.Sum(c => c.Activa1 ?? 0);
                        var totalP2 = consumosOrdenados.Sum(c => c.Activa2 ?? 0);
                        var totalP3 = consumosOrdenados.Sum(c => c.Activa3 ?? 0);
                        var totalGeneral = totalP1 + totalP2 + totalP3;

                        column.Item()
                            .Border(1)
                            .BorderColor(Colors.Orange.Medium)
                            
                            .Background(Colors.Yellow.Lighten1)
                            .Padding(10)
                            .Column(distColumn =>
                            {
                                distColumn.Item().PaddingBottom(8).Text("DISTRIBUCIÓN POR PERIODOS (Acumulado)")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Black);

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

                                // Gráfico de barras horizontales
                                distColumn.Item().PaddingTop(10).Column(grafCol =>
                                {
                                    // P1
                                    grafCol.Item().PaddingBottom(3).Row(row =>
                                    {
                                        row.ConstantItem(30).Text("P1:").FontSize(8).Bold().FontColor(Colors.Black);
                                        row.RelativeItem().PaddingLeft(5).Row(barRow =>
                                        {
                                            var widthP1 = totalGeneral > 0 ? (float)(totalP1 / totalGeneral) : 0;
                                            if (widthP1 > 0)
                                            {
                                                barRow.RelativeItem(widthP1).Height(15).Background(Colors.Orange.Medium);
                                                barRow.RelativeItem(1 - widthP1).Height(15).Background(Colors.Grey.Lighten3);
                                            }
                                            else
                                            {
                                                barRow.RelativeItem().Height(15).Background(Colors.Grey.Lighten3);
                                            }
                                        });
                                    });

                                    // P2
                                    grafCol.Item().PaddingBottom(3).Row(row =>
                                    {
                                        row.ConstantItem(30).Text("P2:").FontSize(8).Bold().FontColor(Colors.Black);
                                        row.RelativeItem().PaddingLeft(5).Row(barRow =>
                                        {
                                            var widthP2 = totalGeneral > 0 ? (float)(totalP2 / totalGeneral) : 0;
                                            if (widthP2 > 0)
                                            {
                                                barRow.RelativeItem(widthP2).Height(15).Background(Colors.Orange.Lighten1);
                                                barRow.RelativeItem(1 - widthP2).Height(15).Background(Colors.Grey.Lighten3);
                                            }
                                            else
                                            {
                                                barRow.RelativeItem().Height(15).Background(Colors.Grey.Lighten3);
                                            }
                                        });
                                    });

                                    // P3
                                    grafCol.Item().Row(row =>
                                    {
                                        row.ConstantItem(30).Text("P3:").FontSize(8).Bold().FontColor(Colors.Black);
                                        row.RelativeItem().PaddingLeft(5).Row(barRow =>
                                        {
                                            var widthP3 = totalGeneral > 0 ? (float)(totalP3 / totalGeneral) : 0;
                                            if (widthP3 > 0)
                                            {
                                                barRow.RelativeItem(widthP3).Height(15).Background(Colors.Orange.Medium);
                                                barRow.RelativeItem(1 - widthP3).Height(15).Background(Colors.Grey.Lighten3);
                                            }
                                            else
                                            {
                                                barRow.RelativeItem().Height(15).Background(Colors.Grey.Lighten3);
                                            }
                                        });
                                    });
                                });
                            });

                        column.Item().PaddingTop(10);

                        // Gráfico de evolución mensual (últimos 6 meses - barras horizontales)
                        var ultimos6Meses = consumosOrdenados.Take(6).ToList();
                        if (ultimos6Meses.Any())
                        {
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Orange.Darken1)
                                
                                .Background(Colors.Yellow.Lighten1)
                                .Padding(10)
                                .Column(graficoColumn =>
                                {
                                    graficoColumn.Item().PaddingBottom(8).Text("EVOLUCIÓN DEL CONSUMO (Últimos 6 meses)")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.Black);

                                    var maxConsumo = ultimos6Meses.Max(c => c.TotalConsumo);
                                    
                                    foreach (var consumo in ultimos6Meses.Reverse<ConsumoSips>())
                                    {
                                        graficoColumn.Item().PaddingBottom(3).Row(row =>
                                        {
                                            row.ConstantItem(60).Text(consumo.FechaInicio?.ToString("MMM yyyy") ?? "").FontSize(7).FontColor(Colors.Grey.Darken1);
                                            row.RelativeItem().PaddingLeft(5).Row(barRow =>
                                            {
                                                var widthPercent = maxConsumo > 0 ? (float)(consumo.TotalConsumo / maxConsumo) : 0;
                                                if (widthPercent > 0)
                                                {
                                                    barRow.RelativeItem(widthPercent).Height(12).Background(Colors.Orange.Medium);
                                                    barRow.RelativeItem(1 - widthPercent).Height(12).Background(Colors.Grey.Lighten3);
                                                }
                                                else
                                                {
                                                    barRow.RelativeItem().Height(12).Background(Colors.Grey.Lighten3);
                                                }
                                            });
                                            row.ConstantItem(70).AlignRight().Text($"{consumo.TotalConsumo:N0} kWh").FontSize(7).FontColor(Colors.Grey.Darken1);
                                        });
                                    }

                                    graficoColumn.Item().PaddingTop(5).Text("Cada barra representa el consumo total mensual en kWh").FontSize(7).FontColor(Colors.Grey.Darken1).Italic();
                                });

                            column.Item().PaddingTop(10);
                        }

                        // Histórico de Consumos (Tabla) - Solo últimos 12 meses
                        var ultimos12MesesTabla = consumosOrdenados.Take(12).ToList();
                        var totalRegistrosCompletos = consumosOrdenados.Count;
                        
                        column.Item().PaddingBottom(5).Text($"HISTÓRICO DE CONSUMOS - Últimos 12 meses ({ultimos12MesesTabla.Count} de {totalRegistrosCompletos} registros)")
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

                            // Datos - Solo últimos 12 meses
                            foreach (var c in ultimos12MesesTabla)
                            {
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(txt => txt.Span(c.FechaInicio?.ToString("MMM yyyy") ?? "").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(txt => txt.Span(c.FechaInicio?.ToString("dd/MM/yy") ?? "").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(txt => txt.Span(c.FechaFin?.ToString("dd/MM/yy") ?? "").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text(txt => txt.Span($"{c.Activa1 ?? 0:N0}").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text(txt => txt.Span($"{c.Activa2 ?? 0:N0}").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text(txt => txt.Span($"{c.Activa3 ?? 0:N0}").FontSize(7));
                                table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text(txt => txt.Span($"{c.TotalConsumo:N0}").FontSize(7).Bold());
                            }

                            // Totales - Solo de los últimos 12 meses mostrados
                            var totalP1_12meses = ultimos12MesesTabla.Sum(c => c.Activa1 ?? 0);
                            var totalP2_12meses = ultimos12MesesTabla.Sum(c => c.Activa2 ?? 0);
                            var totalP3_12meses = ultimos12MesesTabla.Sum(c => c.Activa3 ?? 0);
                            var totalGeneral_12meses = totalP1_12meses + totalP2_12meses + totalP3_12meses;
                            
                            table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten2).Padding(5).Text(txt => txt.Span("TOTAL (Últimos 12 meses)").Bold().FontSize(8));
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(txt => txt.Span($"{totalP1_12meses:N0}").Bold().FontSize(8));
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(txt => txt.Span($"{totalP2_12meses:N0}").Bold().FontSize(8));
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(txt => txt.Span($"{totalP3_12meses:N0}").Bold().FontSize(8));
                            table.Cell().Background(Colors.Green.Lighten2).Padding(5).AlignRight().Text(txt => txt.Span($"{totalGeneral_12meses:N0}").Bold().FontSize(8));
                        });
                        }
                        else
                        {
                            // Mensaje cuando no hay consumos disponibles
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Orange.Lighten1)
                                
                                .Background(Colors.Yellow.Lighten4)
                                .Padding(15)
                                .Column(alertColumn =>
                                {
                                    alertColumn.Item().AlignCenter().Text("⚠️ INFORMACIÓN DE CONSUMOS NO DISPONIBLE")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                    
                                    alertColumn.Item().PaddingTop(8).AlignCenter().Text("No se encontraron datos históricos de consumo para este CUPS.")
                                        .FontSize(9)
                                        .FontColor(Colors.Grey.Darken1);
                                    
                                    alertColumn.Item().PaddingTop(5).AlignCenter().Text("Es posible que sea un suministro nuevo o que no haya lecturas registradas en el sistema.")
                                        .FontSize(8)
                                        .Italic()
                                        .FontColor(Colors.Grey.Medium);
                                });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Column(column =>
                    {
                        column.Item()
                            
                            .Background(Colors.Orange.Darken2)
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
