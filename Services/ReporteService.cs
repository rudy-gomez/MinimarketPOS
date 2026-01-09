using System.Data.SQLite;
using MinimarketPOS.Database;
using MinimarketPOS.Models;

namespace MinimarketPOS.Services
{
    public class ReporteService
    {
        // Modelo para métricas generales
        public class MetricasVentas
        {
            public decimal TotalVentas { get; set; }
            public int NumeroTransacciones { get; set; }
            public decimal TicketPromedio { get; set; }
            public int ProductosVendidos { get; set; }
        }

        // Modelo para venta por empleado
        public class VentaPorEmpleado
        {
            public string NombreEmpleado { get; set; }
            public int NumeroVentas { get; set; }
            public decimal TotalVendido { get; set; }
        }

        // Modelo para producto más vendido
        public class ProductoMasVendido
        {
            public string NombreProducto { get; set; }
            public int CantidadVendida { get; set; }
            public decimal TotalGenerado { get; set; }
        }

        // NUEVO: Modelo para venta con detalles resumidos
        public class VentaDetallada
        {
            public int Id { get; set; }
            public int NumeroTicket { get; set; }
            public DateTime FechaHora { get; set; }
            public string ProductosResumen { get; set; }
            public int CantidadItems { get; set; }
            public decimal Total { get; set; }
            public string NombreUsuario { get; set; }
            public string Estado { get; set; }
        }

        // Obtener métricas de ventas por período
        public static MetricasVentas ObtenerMetricas(DateTime fechaInicio, DateTime fechaFin, int? usuarioId = null)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string condicionUsuario = usuarioId.HasValue ? "AND usuario_id = @usuarioId" : "";

                    string query = $@"
                        SELECT 
                            COALESCE(SUM(total), 0) as total_ventas,
                            COUNT(*) as num_transacciones,
                            COALESCE(AVG(total), 0) as ticket_promedio,
                            COALESCE(SUM(
                                (SELECT SUM(cantidad) FROM detalle_ventas WHERE venta_id = ventas.id)
                            ), 0) as productos_vendidos
                        FROM ventas
                        WHERE DATE(fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                        AND estado = 'completada'
                        {condicionUsuario}";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));

                        if (usuarioId.HasValue)
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new MetricasVentas
                                {
                                    TotalVentas = Convert.ToDecimal(reader["total_ventas"]),
                                    NumeroTransacciones = Convert.ToInt32(reader["num_transacciones"]),
                                    TicketPromedio = Convert.ToDecimal(reader["ticket_promedio"]),
                                    ProductosVendidos = Convert.ToInt32(reader["productos_vendidos"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener métricas: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return new MetricasVentas();
        }

        // Obtener ventas por empleado
        public static List<VentaPorEmpleado> ObtenerVentasPorEmpleado(DateTime fechaInicio, DateTime fechaFin)
        {
            List<VentaPorEmpleado> resultado = new List<VentaPorEmpleado>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            u.nombre as nombre_empleado,
                            COUNT(v.id) as num_ventas,
                            COALESCE(SUM(v.total), 0) as total_vendido
                        FROM ventas v
                        INNER JOIN usuarios u ON v.usuario_id = u.id
                        WHERE DATE(v.fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                        AND v.estado = 'completada'
                        GROUP BY u.id, u.nombre
                        ORDER BY total_vendido DESC";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultado.Add(new VentaPorEmpleado
                                {
                                    NombreEmpleado = reader["nombre_empleado"].ToString(),
                                    NumeroVentas = Convert.ToInt32(reader["num_ventas"]),
                                    TotalVendido = Convert.ToDecimal(reader["total_vendido"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener ventas por empleado: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return resultado;
        }

        // Obtener productos más vendidos
        public static List<ProductoMasVendido> ObtenerProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int limite = 10, int? usuarioId = null)
        {
            List<ProductoMasVendido> resultado = new List<ProductoMasVendido>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string condicionUsuario = usuarioId.HasValue ? "AND v.usuario_id = @usuarioId" : "";

                    string query = $@"
                        SELECT 
                            p.nombre as nombre_producto,
                            SUM(dv.cantidad) as cantidad_vendida,
                            SUM(dv.subtotal) as total_generado
                        FROM detalle_ventas dv
                        INNER JOIN ventas v ON dv.venta_id = v.id
                        INNER JOIN productos p ON dv.producto_id = p.id
                        WHERE DATE(v.fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                        AND v.estado = 'completada'
                        {condicionUsuario}
                        GROUP BY p.id, p.nombre
                        ORDER BY cantidad_vendida DESC
                        LIMIT @limite";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@limite", limite);

                        if (usuarioId.HasValue)
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultado.Add(new ProductoMasVendido
                                {
                                    NombreProducto = reader["nombre_producto"].ToString(),
                                    CantidadVendida = Convert.ToInt32(reader["cantidad_vendida"]),
                                    TotalGenerado = Convert.ToDecimal(reader["total_generado"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener productos más vendidos: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return resultado;
        }

        // NUEVO: Contar total de ventas
        public static int ContarVentas(DateTime fechaInicio, DateTime fechaFin, int? usuarioId = null)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string condicionUsuario = usuarioId.HasValue ? "AND usuario_id = @usuarioId" : "";

                    string query = $@"
                        SELECT COUNT(*) 
                        FROM ventas 
                        WHERE DATE(fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                        {condicionUsuario}";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));

                        if (usuarioId.HasValue)
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId.Value);

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al contar ventas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        // Obtener historial detallado con paginación
        public static List<VentaDetallada> ObtenerHistorialVentasDetallado(DateTime fechaInicio, DateTime fechaFin, int? usuarioId = null, int limite = 50, int offset = 0)
        {
            List<VentaDetallada> ventas = new List<VentaDetallada>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string condicionUsuario = usuarioId.HasValue ? "AND v.usuario_id = @usuarioId" : "";

                    string query = $@"
                        SELECT 
                            v.id,
                            v.numero_ticket,
                            v.fecha_hora,
                            v.total,
                            u.nombre as nombre_usuario,
                            v.estado,
                            (SELECT GROUP_CONCAT(p.nombre, ', ') 
                             FROM detalle_ventas dv 
                             INNER JOIN productos p ON dv.producto_id = p.id 
                             WHERE dv.venta_id = v.id 
                             LIMIT 2) as productos_resumen,
                            (SELECT SUM(cantidad) 
                             FROM detalle_ventas 
                             WHERE venta_id = v.id) as cantidad_items
                        FROM ventas v
                        INNER JOIN usuarios u ON v.usuario_id = u.id
                        WHERE DATE(v.fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                        {condicionUsuario}
                        ORDER BY v.fecha_hora ASC
                        LIMIT @limite OFFSET @offset";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@limite", limite);
                        cmd.Parameters.AddWithValue("@offset", offset);

                        if (usuarioId.HasValue)
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productosResumen = reader["productos_resumen"]?.ToString() ?? "";
                                int cantItems = Convert.ToInt32(reader["cantidad_items"]);

                                if (cantItems > 2 && !string.IsNullOrEmpty(productosResumen))
                                {
                                    productosResumen += $", ... (+{cantItems - 2} más)";
                                }

                                ventas.Add(new VentaDetallada
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    NumeroTicket = Convert.ToInt32(reader["numero_ticket"]),
                                    FechaHora = Convert.ToDateTime(reader["fecha_hora"]),
                                    ProductosResumen = productosResumen,
                                    CantidadItems = cantItems,
                                    Total = Convert.ToDecimal(reader["total"]),
                                    NombreUsuario = reader["nombre_usuario"].ToString(),
                                    Estado = reader["estado"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener historial: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ventas;
        }

        // NUEVO: Obtener detalle de una venta específica
        public static List<DetalleVenta> ObtenerDetalleVenta(int ventaId)
        {
            List<DetalleVenta> detalles = new List<DetalleVenta>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            p.nombre as NombreProducto,
                            dv.cantidad as Cantidad,
                            dv.precio_unitario as PrecioUnitario,
                            dv.subtotal as Subtotal
                        FROM detalle_ventas dv
                        INNER JOIN productos p ON dv.producto_id = p.id
                        WHERE dv.venta_id = @ventaId";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ventaId", ventaId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                detalles.Add(new DetalleVenta
                                {
                                    NombreProducto = reader["NombreProducto"].ToString(),
                                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                    PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                                    Subtotal = Convert.ToDecimal(reader["Subtotal"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener detalle de venta: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return detalles;
        }

        // NUEVO: Anular venta
        public static bool AnularVenta(int ventaId, string motivo, int usuarioId)
        {
            SQLiteConnection? connection = null;
            SQLiteTransaction? transaction = null;

            try
            {
                connection = DatabaseManager.GetConnection();
                connection.Open();
                transaction = connection.BeginTransaction();

                // 1. Obtener detalles de la venta para restaurar stock
                List<(int productoId, int cantidad)> detallesVenta = new List<(int, int)>();

                string queryDetalles = "SELECT producto_id, cantidad FROM detalle_ventas WHERE venta_id = @ventaId";
                using (var cmd = new SQLiteCommand(queryDetalles, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@ventaId", ventaId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detallesVenta.Add((
                                Convert.ToInt32(reader["producto_id"]),
                                Convert.ToInt32(reader["cantidad"])
                            ));
                        }
                    }
                }

                // 2. Restaurar stock de productos
                foreach (var (productoId, cantidad) in detallesVenta)
                {
                    string queryStock = "UPDATE productos SET stock = stock + @cantidad WHERE id = @productoId";
                    using (var cmd = new SQLiteCommand(queryStock, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@cantidad", cantidad);
                        cmd.Parameters.AddWithValue("@productoId", productoId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // 3. Actualizar estado de la venta
                string queryAnular = @"
                    UPDATE ventas 
                    SET estado = 'anulada', 
                        motivo_anulacion = @motivo 
                    WHERE id = @ventaId";

                using (var cmd = new SQLiteCommand(queryAnular, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@motivo", motivo);
                    cmd.Parameters.AddWithValue("@ventaId", ventaId);
                    cmd.ExecuteNonQuery();
                }

                // 4. Registrar auditoría
                AuditoriaService.RegistrarAccion(usuarioId, "anular_venta", "ventas", ventaId,
                    $"Venta anulada. Motivo: {motivo}");

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show($"Error al anular venta: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                connection?.Close();
            }
        }
        // NUEVO: Filtrar ventas por estado y búsqueda
        public static List<VentaDetallada> FiltrarVentas(
            DateTime fechaInicio,
            DateTime fechaFin,
            int? usuarioId,
            string filtroEstado,  // "todas", "completada", "anulada"
            string busquedaProducto,
            int limite,
            int offset)
        {
            List<VentaDetallada> ventas = new List<VentaDetallada>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string condicionUsuario = usuarioId.HasValue ? "AND v.usuario_id = @usuarioId" : "";
                    string condicionEstado = filtroEstado != "todas" ? "AND v.estado = @estado" : "";
                    string condicionBusqueda = !string.IsNullOrWhiteSpace(busquedaProducto)
                        ? @"AND EXISTS (
                    SELECT 1 FROM detalle_ventas dv2 
                    INNER JOIN productos p2 ON dv2.producto_id = p2.id 
                    WHERE dv2.venta_id = v.id 
                    AND p2.nombre LIKE @busqueda
                )" : "";

                    string query = $@"
                SELECT 
                    v.id,
                    v.numero_ticket,
                    v.fecha_hora,
                    v.total,
                    u.nombre as nombre_usuario,
                    v.estado,
                    (SELECT GROUP_CONCAT(p.nombre, ', ') 
                     FROM detalle_ventas dv 
                     INNER JOIN productos p ON dv.producto_id = p.id 
                     WHERE dv.venta_id = v.id 
                     LIMIT 2) as productos_resumen,
                    (SELECT SUM(cantidad) 
                     FROM detalle_ventas 
                     WHERE venta_id = v.id) as cantidad_items
                FROM ventas v
                INNER JOIN usuarios u ON v.usuario_id = u.id
                WHERE DATE(v.fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                {condicionUsuario}
                {condicionEstado}
                {condicionBusqueda}
                ORDER BY v.fecha_hora ASC
                LIMIT @limite OFFSET @offset";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@limite", limite);
                        cmd.Parameters.AddWithValue("@offset", offset);

                        if (usuarioId.HasValue)
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId.Value);

                        if (filtroEstado != "todas")
                            cmd.Parameters.AddWithValue("@estado", filtroEstado);

                        if (!string.IsNullOrWhiteSpace(busquedaProducto))
                            cmd.Parameters.AddWithValue("@busqueda", $"%{busquedaProducto}%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productosResumen = reader["productos_resumen"]?.ToString() ?? "";
                                int cantItems = Convert.ToInt32(reader["cantidad_items"]);

                                if (cantItems > 2 && !string.IsNullOrEmpty(productosResumen))
                                {
                                    productosResumen += ", ...";
                                }

                                ventas.Add(new VentaDetallada
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    NumeroTicket = Convert.ToInt32(reader["numero_ticket"]),
                                    FechaHora = Convert.ToDateTime(reader["fecha_hora"]),
                                    ProductosResumen = productosResumen,
                                    CantidadItems = cantItems,
                                    Total = Convert.ToDecimal(reader["total"]),
                                    NombreUsuario = reader["nombre_usuario"].ToString(),
                                    Estado = reader["estado"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar ventas: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ventas;
        }

        // NUEVO: Contar ventas con filtros
        public static int ContarVentasFiltradas(
            DateTime fechaInicio,
            DateTime fechaFin,
            int? usuarioId,
            string filtroEstado,
            string busquedaProducto)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string condicionUsuario = usuarioId.HasValue ? "AND v.usuario_id = @usuarioId" : "";
                    string condicionEstado = filtroEstado != "todas" ? "AND v.estado = @estado" : "";
                    string condicionBusqueda = !string.IsNullOrWhiteSpace(busquedaProducto)
                        ? @"AND EXISTS (
                    SELECT 1 FROM detalle_ventas dv2 
                    INNER JOIN productos p2 ON dv2.producto_id = p2.id 
                    WHERE dv2.venta_id = v.id 
                    AND p2.nombre LIKE @busqueda
                )" : "";

                    string query = $@"
                SELECT COUNT(*) 
                FROM ventas v
                WHERE DATE(v.fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                {condicionUsuario}
                {condicionEstado}
                {condicionBusqueda}";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));

                        if (usuarioId.HasValue)
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId.Value);

                        if (filtroEstado != "todas")
                            cmd.Parameters.AddWithValue("@estado", filtroEstado);

                        if (!string.IsNullOrWhiteSpace(busquedaProducto))
                            cmd.Parameters.AddWithValue("@busqueda", $"%{busquedaProducto}%");

                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al contar ventas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }
        // NUEVO: Obtener ventas con TODOS los productos para exportación
        public static List<VentaDetallada> ObtenerVentasParaExportacion(
            DateTime fechaInicio,
            DateTime fechaFin,
            int? usuarioId = null)
        {
            List<VentaDetallada> ventas = new List<VentaDetallada>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string condicionUsuario = usuarioId.HasValue ? "AND v.usuario_id = @usuarioId" : "";

                    string query = $@"
                SELECT 
                    v.id,
                    v.numero_ticket,
                    v.fecha_hora,
                    v.total,
                    u.nombre as nombre_usuario,
                    v.estado,
                    (SELECT GROUP_CONCAT(p.nombre, ', ') 
                     FROM detalle_ventas dv 
                     INNER JOIN productos p ON dv.producto_id = p.id 
                     WHERE dv.venta_id = v.id) as productos_resumen,
                    (SELECT SUM(cantidad) 
                     FROM detalle_ventas 
                     WHERE venta_id = v.id) as cantidad_items
                FROM ventas v
                INNER JOIN usuarios u ON v.usuario_id = u.id
                WHERE DATE(v.fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                {condicionUsuario}
                ORDER BY v.fecha_hora ASC";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));

                        if (usuarioId.HasValue)
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ventas.Add(new VentaDetallada
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    NumeroTicket = Convert.ToInt32(reader["numero_ticket"]),
                                    FechaHora = Convert.ToDateTime(reader["fecha_hora"]),
                                    ProductosResumen = reader["productos_resumen"]?.ToString() ?? "",
                                    CantidadItems = Convert.ToInt32(reader["cantidad_items"]),
                                    Total = Convert.ToDecimal(reader["total"]),
                                    NombreUsuario = reader["nombre_usuario"].ToString(),
                                    Estado = reader["estado"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener ventas para exportación: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ventas;
        }
    }
}