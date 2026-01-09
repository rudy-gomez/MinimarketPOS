using ClosedXML.Excel;
using static MinimarketPOS.Services.ReporteService;

namespace MinimarketPOS.Utils
{
    public class ExportarExcelService
    {
        public static void ExportarHistorialVentas(List<VentaDetallada> ventas, string nombreArchivo, bool esAdmin)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Historial Ventas");

                    // Encabezados
                    worksheet.Cell(1, 1).Value = "TICKET";
                    worksheet.Cell(1, 2).Value = "FECHA/HORA";
                    worksheet.Cell(1, 3).Value = "PRODUCTOS";
                    worksheet.Cell(1, 4).Value = "ITEMS";
                    worksheet.Cell(1, 5).Value = "TOTAL";
                    if (esAdmin)
                        worksheet.Cell(1, 6).Value = "CAJERO";
                    worksheet.Cell(1, esAdmin ? 7 : 6).Value = "ESTADO";

                    // Estilo encabezado
                    var headerRange = worksheet.Range(1, 1, 1, esAdmin ? 7 : 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#007BFF");
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Datos
                    int row = 2;
                    foreach (var venta in ventas)
                    {
                        worksheet.Cell(row, 1).Value = $"#{venta.NumeroTicket:D6}";
                        worksheet.Cell(row, 2).Value = venta.FechaHora.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cell(row, 3).Value = venta.ProductosResumen;
                        worksheet.Cell(row, 4).Value = venta.CantidadItems;
                        worksheet.Cell(row, 5).Value = (double)venta.Total;
                        worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

                        if (esAdmin)
                            worksheet.Cell(row, 6).Value = venta.NombreUsuario;

                        int estadoCol = esAdmin ? 7 : 6;
                        worksheet.Cell(row, estadoCol).Value = venta.Estado.ToUpper();

                        // Color según estado
                        if (venta.Estado == "completada")
                            worksheet.Cell(row, estadoCol).Style.Fill.BackgroundColor = XLColor.FromHtml("#D4EDDA");
                        else if (venta.Estado == "anulada")
                            worksheet.Cell(row, estadoCol).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8D7DA");

                        row++;
                    }

                    // Ajustar anchos
                    worksheet.Columns().AdjustToContents();

                    // Guardar
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "Archivo Excel|*.xlsx";
                    saveDialog.FileName = nombreArchivo;

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        workbook.SaveAs(saveDialog.FileName);
                        MessageBox.Show("✅ Archivo Excel generado correctamente",
                                      "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar Excel: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}