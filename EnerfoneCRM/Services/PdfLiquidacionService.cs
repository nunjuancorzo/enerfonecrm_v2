using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EnerfoneCRM.Models;

namespace EnerfoneCRM.Services;

public class PdfLiquidacionService
{
    public byte[] GenerarPdfLiquidacion(
        Usuario usuario,
        List<Contrato> contratosEnergia,
        List<Contrato> contratosTelefonia,
        List<Contrato> contratosAlarmas,
        decimal totalComisiones,
        ConfiguracionEmpresa? configuracion = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var fechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        var totalContratos = contratosEnergia.Count + contratosTelefonia.Count + contratosAlarmas.Count;

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
                        // Logo de la empresa si existe
                        if (configuracion != null && !string.IsNullOrEmpty(configuracion.LogoUrl))
                        {
                            try
                            {
                                byte[]? logoBytes = null;
                                
                                // Verificar si el logo es un data URL base64
                                if (configuracion.LogoUrl.StartsWith("data:"))
                                {
                                    // Extraer el base64 del data URL
                                    var base64Data = configuracion.LogoUrl.Split(',')[1];
                                    logoBytes = Convert.FromBase64String(base64Data);
                                }
                                else
                                {
                                    // Es una ruta de archivo
                                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", configuracion.LogoUrl.TrimStart('/'));
                                    if (File.Exists(logoPath))
                                    {
                                        logoBytes = File.ReadAllBytes(logoPath);
                                    }
                                }

                                // Mostrar el logo si se obtuvieron los bytes
                                if (logoBytes != null && logoBytes.Length > 0)
                                {
                                    column.Item().AlignCenter().Height(60).Image(logoBytes);
                                    column.Item().PaddingTop(10);
                                }
                            }
                            catch
                            {
                                // Si hay error al cargar el logo, simplemente no lo mostramos
                            }
                        }

                        column.Item().Text("LIQUIDACIÓN DE CONTRATOS")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2)
                            .AlignCenter();

                        // Información de la empresa si existe
                        if (configuracion != null)
                        {
                            column.Item().PaddingTop(5).Text(configuracion.NombreEmpresa)
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1)
                                .AlignCenter();
                            
                            if (!string.IsNullOrEmpty(configuracion.Cif))
                            {
                                column.Item().Text($"CIF: {configuracion.Cif}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1)
                                    .AlignCenter();
                            }
                        }

                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        // Información del usuario
                        column.Item()
                            .Background(Colors.Grey.Lighten3)
                            .Padding(10)
                            .Column(infoColumn =>
                            {
                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Usuario: ").Bold();
                                        txt.Span(usuario.NombreUsuario);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Nombre: ").Bold();
                                        txt.Span($"{usuario.Nombre} {usuario.Apellidos}");
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Email: ").Bold();
                                        txt.Span(usuario.Email ?? "");
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Fecha de emisión: ").Bold();
                                        txt.Span(fechaActual);
                                    });
                                });

                                infoColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(txt =>
                                    {
                                        txt.Span("Total contratos: ").Bold();
                                        txt.Span(totalContratos.ToString());
                                    });
                                });
                            });

                        column.Item().PaddingTop(15);

                        // Contratos de Energía
                        if (contratosEnergia.Any())
                        {
                            column.Item().Text($"CONTRATOS DE ENERGÍA ({contratosEnergia.Count})")
                                .FontSize(12)
                                .Bold()
                                .FontColor(Colors.Grey.Darken2);

                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.5f); // Fecha
                                    columns.RelativeColumn(2.5f); // Cliente
                                    columns.RelativeColumn(1.5f); // DNI
                                    columns.RelativeColumn(2f);   // Comercializadora
                                    columns.RelativeColumn(2f);   // Tarifa
                                    columns.RelativeColumn(2f);   // CUPS Luz
                                    columns.RelativeColumn(2f);   // CUPS Gas
                                    columns.RelativeColumn(1.5f); // Comisión
                                });

                                // Encabezado
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("Fecha").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("Cliente").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("DNI").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("Comercial.").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("Tarifa").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("CUPS Luz").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("CUPS Gas").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("Comisión").FontColor(Colors.White).Bold().FontSize(9);
                                });

                                // Datos
                                foreach (var c in contratosEnergia)
                                {
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.FechaCreacion?.ToString("dd/MM/yyyy") ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.NombreCliente ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.Dni ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.EnComercializadora ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.EnTarifa ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.EnCups ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.EnCupsGas ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text($"{(c.Comision ?? 0):N2}€").FontSize(8);
                                }
                            });

                            column.Item().PaddingTop(5).AlignRight().Text($"Subtotal Energía: {contratosEnergia.Sum(c => c.Comision ?? 0):N2}€").Bold();
                            column.Item().PaddingTop(10);
                        }

                        // Contratos de Telefonía
                        if (contratosTelefonia.Any())
                        {
                            column.Item().Text($"CONTRATOS DE TELEFONÍA ({contratosTelefonia.Count})")
                                .FontSize(12)
                                .Bold()
                                .FontColor(Colors.Grey.Darken2);

                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.5f); // Fecha
                                    columns.RelativeColumn(2.5f); // Cliente
                                    columns.RelativeColumn(1.5f); // DNI
                                    columns.RelativeColumn(2f);   // Operadora
                                    columns.RelativeColumn(2f);   // Tarifa
                                    columns.RelativeColumn(1.5f); // Fijo
                                    columns.RelativeColumn(2f);   // Móvil
                                    columns.RelativeColumn(1f);   // Nº Líneas
                                    columns.RelativeColumn(1.5f); // Comisión
                                });

                                // Encabezado
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Fecha").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Cliente").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("DNI").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Operadora").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Tarifa").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Fijo").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Móvil").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Líneas").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Comisión").FontColor(Colors.White).Bold().FontSize(9);
                                });

                                // Datos
                                foreach (var c in contratosTelefonia)
                                {
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.FechaCreacion?.ToString("dd/MM/yyyy") ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.NombreCliente ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.Dni ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.OperadoraTel ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.TarifaTel ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.FijoTel ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.LineaMovilPrincipal ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.NumeroLineasTel?.ToString() ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text($"{(c.Comision ?? 0):N2}€").FontSize(8);
                                }
                            });

                            column.Item().PaddingTop(5).AlignRight().Text($"Subtotal Telefonía: {contratosTelefonia.Sum(c => c.Comision ?? 0):N2}€").Bold();
                            column.Item().PaddingTop(10);
                        }

                        // Contratos de Alarmas
                        if (contratosAlarmas.Any())
                        {
                            column.Item().Text($"CONTRATOS DE ALARMAS ({contratosAlarmas.Count})")
                                .FontSize(12)
                                .Bold()
                                .FontColor(Colors.Grey.Darken2);

                            column.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.5f); // Fecha
                                    columns.RelativeColumn(2.5f); // Cliente
                                    columns.RelativeColumn(1.5f); // DNI
                                    columns.RelativeColumn(2f);   // Tipo
                                    columns.RelativeColumn(2f);   // Subtipo
                                    columns.RelativeColumn(1.5f); // Kit
                                    columns.RelativeColumn(2f);   // Campaña
                                    columns.RelativeColumn(1.5f); // Comisión
                                });

                                // Encabezado
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Red.Darken1).Padding(5).Text("Fecha").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Red.Darken1).Padding(5).Text("Cliente").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Red.Darken1).Padding(5).Text("DNI").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Red.Darken1).Padding(5).Text("Tipo").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Red.Darken1).Padding(5).Text("Subtipo").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Red.Darken1).Padding(5).Text("Kit").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Red.Darken1).Padding(5).Text("Campaña").FontColor(Colors.White).Bold().FontSize(9);
                                    header.Cell().Background(Colors.Red.Darken1).Padding(5).Text("Comisión").FontColor(Colors.White).Bold().FontSize(9);
                                });

                                // Datos
                                foreach (var c in contratosAlarmas)
                                {
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.FechaCreacion?.ToString("dd/MM/yyyy") ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.NombreCliente ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.Dni ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.TipoAlarma ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.SubtipoInmueble ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.KitAlarma ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(c.CampanaAlarma ?? "").FontSize(8);
                                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).AlignRight().Text($"{(c.Comision ?? 0):N2}€").FontSize(8);
                                }
                            });

                            column.Item().PaddingTop(5).AlignRight().Text($"Subtotal Alarmas: {contratosAlarmas.Sum(c => c.Comision ?? 0):N2}€").Bold();
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Column(column =>
                    {
                        column.Item()
                            .Background(Colors.Blue.Darken2)
                            .Padding(10)
                            .AlignRight()
                            .Text($"TOTAL A LIQUIDAR: {totalComisiones:N2}€")
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.White);

                        // Nota aclaratoria con información de la empresa
                        if (configuracion != null)
                        {
                            column.Item().PaddingTop(15).Border(2).BorderColor(Colors.Blue.Darken2).Padding(10)
                                .Column(noteColumn =>
                                {
                                    noteColumn.Item().Text("Nota aclaratoria: el total a liquidar es la base sin impuestos, debes generar una factura a:")
                                        .FontSize(9)
                                        .Bold();
                                    
                                    noteColumn.Item().PaddingTop(8).Text(configuracion.NombreEmpresa)
                                        .FontSize(10)
                                        .Bold();
                                    
                                    if (!string.IsNullOrEmpty(configuracion.Cif))
                                    {
                                        noteColumn.Item().Text($"CIF: {configuracion.Cif}")
                                            .FontSize(9);
                                    }
                                    
                                    if (!string.IsNullOrEmpty(configuracion.Direccion))
                                    {
                                        noteColumn.Item().Text(configuracion.Direccion)
                                            .FontSize(9);
                                    }
                                    
                                    if (!string.IsNullOrEmpty(configuracion.CodigoPostal) || !string.IsNullOrEmpty(configuracion.Ciudad))
                                    {
                                        var ubicacion = $"{configuracion.CodigoPostal ?? ""} {configuracion.Ciudad ?? ""}".Trim();
                                        if (!string.IsNullOrEmpty(ubicacion))
                                        {
                                            noteColumn.Item().Text(ubicacion)
                                                .FontSize(9);
                                        }
                                    }
                                    
                                    if (!string.IsNullOrEmpty(configuracion.Telefono) || !string.IsNullOrEmpty(configuracion.Email))
                                    {
                                        var contacto = "Contacto: ";
                                        var partes = new List<string>();
                                        
                                        if (!string.IsNullOrEmpty(configuracion.Telefono))
                                            partes.Add(configuracion.Telefono);
                                        
                                        if (!string.IsNullOrEmpty(configuracion.Email))
                                            partes.Add(configuracion.Email);
                                        
                                        contacto += string.Join(" / ", partes);
                                        
                                        noteColumn.Item().Text(contacto)
                                            .FontSize(9);
                                    }
                                });
                        }

                        column.Item().PaddingTop(10).Text(txt =>
                        {
                            txt.Span("Página ");
                            txt.CurrentPageNumber();
                            txt.Span(" de ");
                            txt.TotalPages();
                        });
                    });
            });
        });

        return documento.GeneratePdf();
    }
}
