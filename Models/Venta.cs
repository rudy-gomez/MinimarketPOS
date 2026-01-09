namespace MinimarketPOS.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public int NumeroTicket { get; set; }
        public DateTime FechaHora { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } // Para mostrar en reportes
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal EfectivoRecibido { get; set; }
        public decimal Vuelto { get; set; }
        public string Estado { get; set; }
        public string MotivoAnulacion { get; set; }

        // Lista de productos vendidos
        public List<DetalleVenta> Detalles { get; set; }

        public Venta()
        {
            FechaHora = DateTime.Now;
            Estado = "completada";
            Detalles = new List<DetalleVenta>();
        }

        // Calcular total de la venta
        public void CalcularTotal()
        {
            Subtotal = Detalles.Sum(d => d.Subtotal);
            Total = Subtotal; 
        }

        // Calcular vuelto
        public void CalcularVuelto()
        {
            Vuelto = EfectivoRecibido - Total;
        }
    }
}