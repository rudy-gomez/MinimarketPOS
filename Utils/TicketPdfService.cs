using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MinimarketPOS.Models;
using MinimarketPOS.Services;

namespace MinimarketPOS.Utils
{
    public class TicketPdfService
    {
        public static void GenerarTicketPdf(int numeroTicket, List<DetalleVenta> detalles,
                                           decimal total, decimal efectivo, decimal vuelto,
                                           string cajero, DateTime fechaHora)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var config = ConfiguracionService.ObtenerTodo();
                string nombreNegocio = config.ContainsKey("nombre_negocio") ? config["nombre_negocio"] : "MINIMARKET";
                string direccion = config.ContainsKey("direccion") ? config["direccion"] : "";
                string ciudad = config.ContainsKey("ciudad") ? config["ciudad"] : "";
                string ruc = config.ContainsKey("ruc") ? config["ruc"] : "";
                string mensaje = config.ContainsKey("mensaje_ticket") ? config["mensaje_ticket"] : "¡GRACIAS POR SU COMPRA!";

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Tamaño de ticket térmico (80mm)
                        page.Size(80, 300, Unit.Millimetre); // ancho x alto
                        page.Margin(5, Unit.Millimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Courier New"));

                        page.Content()
                            .Column(column =>
                            {
                                column.Spacing(3);

                                // Encabezado
                                column.Item().AlignCenter().Text(nombreNegocio)
                                    .Bold().FontSize(12);

                                if (!string.IsNullOrEmpty(direccion))
                                    column.Item().AlignCenter().Text(direccion).FontSize(8);

                                if (!string.IsNullOrEmpty(ciudad))
                                    column.Item().AlignCenter().Text(ciudad).FontSize(8);

                                if (!string.IsNullOrEmpty(ruc))
                                    column.Item().AlignCenter().Text($"RUC: {ruc}").FontSize(8);

                                // Línea separadora
                                column.Item().LineHorizontal(1).LineColor(Colors.Black);

                                // Información del ticket
                                column.Item().Text($"TICKET #{numeroTicket:D6}").Bold();
                                column.Item().Text($"Fecha: {fechaHora:dd/MM/yyyy HH:mm}");
                                column.Item().Text($"Cajero: {cajero}");

                                // Línea separadora
                                column.Item().LineHorizontal(1).LineColor(Colors.Black);

                                // Encabezado de productos
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem(3).Text("PRODUCTO").Bold();
                                    row.RelativeItem(1).AlignCenter().Text("CANT").Bold();
                                    row.RelativeItem(1).AlignRight().Text("TOTAL").Bold();
                                });

                                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                                // Productos
                                foreach (var detalle in detalles)
                                {
                                    column.Item().Row(row =>
                                    {
                                        row.RelativeItem(3).Text(detalle.NombreProducto).FontSize(8);
                                        row.RelativeItem(1).AlignCenter().Text(detalle.Cantidad.ToString());
                                        row.RelativeItem(1).AlignRight().Text($"S/ {detalle.Subtotal:F2}");
                                    });
                                }

                                // Línea separadora
                                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Black);

                                // Subtotal
                                column.Item().AlignRight().Text($"SUBTOTAL: S/ {total:F2}");

                                // Línea separadora
                                column.Item().LineHorizontal(1).LineColor(Colors.Black);

                                // Total
                                column.Item().AlignRight().Text($"TOTAL A PAGAR: S/ {total:F2}")
                                    .Bold().FontSize(11);

                                column.Item().PaddingTop(5);

                                // Efectivo y vuelto
                                column.Item().AlignRight().Text($"Efectivo: S/ {efectivo:F2}");
                                column.Item().AlignRight().Text($"Vuelto: S/ {vuelto:F2}");

                                // Línea separadora final
                                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Black);

                                // Mensaje de despedida
                                column.Item().AlignCenter().Text(mensaje)
                                    .Bold().FontSize(9);

                                column.Item().LineHorizontal(1).LineColor(Colors.Black);
                            });
                    });
                });

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Archivo PDF|*.pdf";
                saveDialog.FileName = $"Ticket_{numeroTicket:D6}.pdf";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    document.GeneratePdf(saveDialog.FileName);
                    MessageBox.Show("✅ Ticket PDF generado correctamente",
                                  "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar PDF: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}