using MinimarketPOS.Models;
using MinimarketPOS.Services;

namespace MinimarketPOS.Utils
{
    public class PrinterService
    {
        // Vista previa del ticket antes de imprimir
        // Cambiar nombre del método
        public static void ImprimirDirecto(int numeroTicket, List<DetalleVenta> detalles,
                                           decimal total, decimal efectivo, decimal vuelto, string cajero)
        {
            try
            {
                string ticketTexto = GenerarTicketTexto(numeroTicket, detalles, total, efectivo, vuelto, cajero);

                // Imprimir usando PrintDocument de Windows
                System.Drawing.Printing.PrintDocument pd = new System.Drawing.Printing.PrintDocument();
                pd.PrintPage += (sender, e) => {
                    // Dibujar el ticket
                    e.Graphics.DrawString(ticketTexto,
                        new Font("Courier New", 8),
                        Brushes.Black,
                        new RectangleF(0, 0, 300, 1000));
                };

                // Intentar imprimir
                pd.Print();

                MessageBox.Show($"✅ Ticket #{numeroTicket:D6} impreso correctamente",
                               "Impresión", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ No se pudo imprimir automáticamente.\n\n{ex.Message}\n\nTicket #{numeroTicket:D6} guardado en el sistema.",
                               "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Generar texto del ticket
        private static string GenerarTicketTexto(int numeroTicket, List<DetalleVenta> detalles,
                                                 decimal total, decimal efectivo, decimal vuelto, string cajero)
        {
            var config = ConfiguracionService.ObtenerTodo();
            string nombreNegocio = config.ContainsKey("nombre_negocio") ? config["nombre_negocio"] : "MINIMARKET";
            string direccion = config.ContainsKey("direccion") ? config["direccion"] : "";
            string ciudad = config.ContainsKey("ciudad") ? config["ciudad"] : "";
            string ruc = config.ContainsKey("ruc") ? config["ruc"] : "";
            string mensaje = config.ContainsKey("mensaje_ticket") ? config["mensaje_ticket"] : "¡GRACIAS POR SU COMPRA!";

            string ticket = "";
            ticket += "================================\n";
            ticket += CentrarTexto(nombreNegocio, 32) + "\n";
            ticket += CentrarTexto(direccion, 32) + "\n";
            ticket += CentrarTexto(ciudad, 32) + "\n";
            if (!string.IsNullOrEmpty(ruc))
                ticket += CentrarTexto($"RUC: {ruc}", 32) + "\n";
            ticket += "================================\n";
            ticket += $"TICKET #{numeroTicket:D6}\n";
            ticket += $"Fecha: {DateTime.Now:dd/MM/yyyy  HH:mm}\n";
            ticket += $"Cajero: {cajero}\n";
            ticket += "--------------------------------\n";
            ticket += "PRODUCTO         CANT   PRECIO\n";
            ticket += "--------------------------------\n";

            foreach (var detalle in detalles)
            {
                string nombre = detalle.NombreProducto.Length > 16 ?
                               detalle.NombreProducto.Substring(0, 16) :
                               detalle.NombreProducto.PadRight(16);

                ticket += $"{nombre} {detalle.Cantidad,4}   S/{detalle.Subtotal,6:F2}\n";
            }

            ticket += "--------------------------------\n";
            ticket += $"              SUBTOTAL: S/{total,6:F2}\n";
            ticket += "--------------------------------\n";
            ticket += $"         TOTAL A PAGAR: S/{total,6:F2}\n\n";
            ticket += $"Efectivo:           S/{efectivo,6:F2}\n";
            ticket += $"Vuelto:             S/{vuelto,6:F2}\n\n";
            ticket += "================================\n";
            ticket += CentrarTexto(mensaje, 32) + "\n";
            ticket += "================================\n";

            return ticket;
        }

        // Imprimir en impresora térmica
        private static void ImprimirTicket(int numeroTicket, List<DetalleVenta> detalles,
                                          decimal total, decimal efectivo, decimal vuelto, string cajero)
        {
            try
            {
                MessageBox.Show(
                    $"✅ Ticket #{numeroTicket:D6} listo para imprimir\n\n" +
                    "Para imprimir en impresora térmica real:\n" +
                    "1. Conecte la impresora POS-H58 por USB\n" +
                    "2. Configure el puerto COM en PrinterService.cs\n" +
                    "3. Descomente el código de impresión",
                    "Impresión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                /* CÓDIGO PARA IMPRESORA TÉRMICA REAL (cuando tengas la impresora):
                
                using ESCPOS_NET.Emitters;
                using ESCPOS_NET;
                
                var printer = new SerialPrinter(portName: "COM3", baudRate: 9600);
                var e = new EPSON();

                printer.Write(
                    e.CenterAlign(),
                    e.PrintLine(nombreNegocio),
                    e.PrintLine(direccion),
                    e.PrintLine("================================"),
                    e.LeftAlign(),
                    e.PrintLine($"TICKET #{numeroTicket:D6}"),
                    e.PrintLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}")
                );

                foreach (var detalle in detalles)
                {
                    printer.Write(e.PrintLine($"{detalle.NombreProducto} x{detalle.Cantidad} S/{detalle.Subtotal:F2}"));
                }

                printer.Write(
                    e.PrintLine("--------------------------------"),
                    e.PrintLine($"TOTAL: S/{total:F2}"),
                    e.PrintLine($"Efectivo: S/{efectivo:F2}"),
                    e.PrintLine($"Vuelto: S/{vuelto:F2}"),
                    e.CenterAlign(),
                    e.PrintLine("¡GRACIAS POR SU COMPRA!"),
                    e.FullPaperCut()
                );
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                              "Error de Impresión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Centrar texto
        private static string CentrarTexto(string texto, int ancho)
        {
            if (texto.Length >= ancho) return texto.Substring(0, ancho);
            int espacios = (ancho - texto.Length) / 2;
            return new string(' ', espacios) + texto;
        }
    }
}