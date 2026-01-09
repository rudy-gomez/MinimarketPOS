namespace MinimarketPOS.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Categoria { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Constructor vacío
        public Producto()
        {
            StockMinimo = 10;
            Activo = true;
        }

        // Método para verificar si está con stock bajo
        public bool TieneStockBajo()
        {
            return Stock <= StockMinimo;
        }

        // Método para verificar si está próximo a vencer (30 días)
        public bool ProximoAVencer()
        {
            if (!FechaVencimiento.HasValue) return false;

            TimeSpan diferencia = FechaVencimiento.Value - DateTime.Now;
            return diferencia.TotalDays <= 30 && diferencia.TotalDays > 0;
        }

        // Método para verificar si está vencido
        public bool EstaVencido()
        {
            if (!FechaVencimiento.HasValue) return false;
            return FechaVencimiento.Value < DateTime.Now;
        }
    }
}