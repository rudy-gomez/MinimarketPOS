using ClosedXML.Excel;
using MinimarketPOS.Models;

namespace MinimarketPOS.Utils
{
    public class ExcelExportService
    {
        public static bool ExportarInventario(List<Producto> productos, string rutaArchivo)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    // ← CONFIGURAR PROPIEDADES DEL WORKBOOK
                    workbook.Properties.Author = "Sistema MinimarketPOS";
                    workbook.Properties.Title = "Inventario de Productos";
                    workbook.Properties.Subject = "Exportación de Inventario";
                    workbook.Properties.Created = DateTime.Now;

                    var worksheet = workbook.Worksheets.Add("Inventario");

                    // Encabezados (agregando # al inicio)
                    worksheet.Cell(1, 1).Value = "#";
                    worksheet.Cell(1, 2).Value = "Código";
                    worksheet.Cell(1, 3).Value = "Nombre";
                    worksheet.Cell(1, 4).Value = "Categoría";
                    worksheet.Cell(1, 5).Value = "Precio";
                    worksheet.Cell(1, 6).Value = "Stock";
                    worksheet.Cell(1, 7).Value = "Stock Mínimo";
                    worksheet.Cell(1, 8).Value = "Fecha Vencimiento";
                    worksheet.Cell(1, 9).Value = "Estado";

                    // Estilo de encabezados
                    var headerRange = worksheet.Range(1, 1, 1, 9); // ← Cambiar de 8 a 9
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(52, 73, 94);
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int row = 2;
                    int numeroFila = 1; // ← AGREGAR contador
                    foreach (var producto in productos)
                    {
                        worksheet.Cell(row, 1).Value = numeroFila; // ← # enumeración
                        worksheet.Cell(row, 2).Value = producto.CodigoBarras;
                        worksheet.Cell(row, 3).Value = producto.Nombre;
                        worksheet.Cell(row, 4).Value = producto.Categoria ?? "";
                        worksheet.Cell(row, 5).Value = (double)producto.Precio;
                        worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(row, 6).Value = producto.Stock;
                        worksheet.Cell(row, 7).Value = producto.StockMinimo;

                        if (producto.FechaVencimiento.HasValue)
                        {
                            worksheet.Cell(row, 8).Value = producto.FechaVencimiento.Value;
                            worksheet.Cell(row, 8).Style.DateFormat.Format = "dd/MM/yyyy";

                            var diasRestantes = (producto.FechaVencimiento.Value - DateTime.Now).Days;
                            if (diasRestantes < 0)
                            {
                                worksheet.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.FromArgb(231, 76, 60);
                                worksheet.Cell(row, 8).Style.Font.FontColor = XLColor.White;
                            }
                            else if (diasRestantes <= 30)
                            {
                                worksheet.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.FromArgb(243, 156, 18);
                            }
                        }
                        else
                        {
                            worksheet.Cell(row, 8).Value = "Sin vencimiento";
                        }

                        worksheet.Cell(row, 9).Value = producto.Activo ? "Activo" : "Inactivo";

                        // Resaltar stock bajo
                        if (producto.Stock <= producto.StockMinimo)
                        {
                            worksheet.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(231, 76, 60);
                            worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.White;
                            worksheet.Cell(row, 6).Style.Font.Bold = true;
                        }

                        row++;
                        numeroFila++; // ← Incrementar contador
                    }

                    // Ajustar anchos
                    worksheet.Columns().AdjustToContents();

                    // Agregar bordes
                    var dataRange = worksheet.Range(1, 1, row - 1, 9);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // ← VALIDAR Y GUARDAR CORRECTAMENTE
                    try
                    {
                        // Eliminar archivo existente si existe
                        if (File.Exists(rutaArchivo))
                        {
                            File.Delete(rutaArchivo);
                            System.Threading.Thread.Sleep(100); // Esperar a que Windows libere el archivo
                        }

                        // Guardar con opciones específicas
                        var saveOptions = new SaveOptions
                        {
                            EvaluateFormulasBeforeSaving = false,
                            GenerateCalculationChain = false
                        };

                        workbook.SaveAs(rutaArchivo, saveOptions);
                        return true;
                    }
                    catch (IOException ioEx)
                    {
                        MessageBox.Show($"No se pudo guardar el archivo. Asegúrese de que no esté abierto en otra aplicación.\n\nError: {ioEx.Message}",
                                       "Error de acceso al archivo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}