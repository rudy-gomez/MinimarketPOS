namespace MinimarketPOS.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } // Para mostrar en ticket
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        public DetalleVenta() { }

        public DetalleVenta(Producto producto, int cantidad)
        {
            ProductoId = producto.Id;
            NombreProducto = producto.Nombre;
            Cantidad = cantidad;
            PrecioUnitario = producto.Precio;
            CalcularSubtotal();
        }

        // Calcular subtotal del detalle
        public void CalcularSubtotal()
        {
            Subtotal = Cantidad * PrecioUnitario;
        }
    }
}