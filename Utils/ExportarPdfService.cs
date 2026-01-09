using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot.WinForms;
using static MinimarketPOS.Services.ReporteService;

namespace MinimarketPOS.Utils
{
    public class ExportarPdfService
    {
        public static void ExportarReporte(
            MetricasVentas metricas,
            List<VentaPorEmpleado> ventasEmpleados,
            List<ProductoMasVendido> topProductos,
            DateTime fechaInicio,
            DateTime fechaFin,
            bool esAdmin,
            FormsPlot? plotVentasPorDia = null,
            FormsPlot? plotTopProductos = null)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header()
                            .Text("■ REPORTE DE VENTAS")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Spacing(20);

                                // Período
                                x.Item().Text($"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}")
                                    .FontSize(12).FontColor(Colors.Grey.Darken2);

                                // Métricas
                                x.Item().Text("RESUMEN GENERAL").Bold().FontSize(14);
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Cell().Background(Colors.Green.Medium)
                                        .Padding(10).Text("● VENTAS TOTALES").Bold().FontColor(Colors.White);
                                    table.Cell().Background(Colors.Blue.Medium)
                                        .Padding(10).Text("▪ TRANSACCIONES").Bold().FontColor(Colors.White);
                                    table.Cell().Background(Colors.Orange.Medium)
                                        .Padding(10).Text("■ TICKET PROMEDIO").Bold().FontColor(Colors.White);
                                    table.Cell().Background(Colors.Purple.Medium)
                                        .Padding(10).Text("▪ PRODUCTOS").Bold().FontColor(Colors.White);

                                    table.Cell().Padding(10).Text($"S/ {metricas.TotalVentas:N2}").Bold().FontSize(14);
                                    table.Cell().Padding(10).Text(metricas.NumeroTransacciones.ToString()).Bold().FontSize(14);
                                    table.Cell().Padding(10).Text($"S/ {metricas.TicketPromedio:N2}").Bold().FontSize(14);
                                    table.Cell().Padding(10).Text(metricas.ProductosVendidos.ToString()).Bold().FontSize(14);
                                });

                                // Ventas por empleado (solo admin)
                                if (esAdmin && ventasEmpleados.Count > 0)
                                {
                                    x.Item().Text("● VENTAS POR EMPLEADO").Bold().FontSize(14);
                                    x.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(2);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("EMPLEADO").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("VENTAS").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("TOTAL").Bold();
                                        });

                                        foreach (var emp in ventasEmpleados)
                                        {
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(emp.NombreEmpleado);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(emp.NumeroVentas.ToString());
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"S/ {emp.TotalVendido:N2}");
                                        }
                                    });
                                }

                                // Top productos
                                x.Item().Text("■ TOP 10 PRODUCTOS MÁS VENDIDOS").Bold().FontSize(14);
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("PRODUCTO").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("CANT.").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("TOTAL").Bold();
                                    });

                                    foreach (var prod in topProductos)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(prod.NombreProducto);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(prod.CantidadVendida.ToString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"S/ {prod.TotalGenerado:N2}");
                                    }
                                });

                                // GRÁFICAS
                                if (plotVentasPorDia != null && plotTopProductos != null)
                                {
                                    x.Item().Text("▪ GRÁFICAS DE ANÁLISIS").Bold().FontSize(14);

                                    string tempPath1 = Path.Combine(Path.GetTempPath(), "grafica_ventas.png");
                                    string tempPath2 = Path.Combine(Path.GetTempPath(), "grafica_productos.png");

                                    try
                                    {
                                        plotVentasPorDia.Plot.SavePng(tempPath1, 600, 300);
                                        plotTopProductos.Plot.SavePng(tempPath2, 600, 300);

                                        x.Item().Image(tempPath1).FitWidth();
                                        x.Item().Image(tempPath2).FitWidth();

                                        File.Delete(tempPath1);
                                        File.Delete(tempPath2);
                                    }
                                    catch
                                    {
                                        x.Item().Text("No se pudieron exportar las gráficas")
                                            .FontSize(10)
                                            .Italic();
                                    }
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                                x.Span(" de ");
                                x.TotalPages();
                            });
                    });
                });

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Archivo PDF|*.pdf";
                saveDialog.FileName = $"Reporte_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.pdf";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    document.GeneratePdf(saveDialog.FileName);
                    MessageBox.Show("Reporte PDF generado correctamente",
                                  "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar PDF: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}