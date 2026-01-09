using System.Data.SQLite;
using MinimarketPOS.Database;
using MinimarketPOS.Models;

namespace MinimarketPOS.Services
{
    public class VentaService
    {
        // Buscar producto por código o nombre
        public static Producto? BuscarProducto(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
                return null;

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    // Buscar por código exacto
                    string query = "SELECT * FROM productos WHERE codigo_barras = @criterio AND activo = 1";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@criterio", criterio);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return MapearProducto(reader);
                        }
                    }

                    // Si no encuentra, buscar por nombre
                    query = "SELECT * FROM productos WHERE nombre LIKE @criterio AND activo = 1 LIMIT 1";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@criterio", $"%{criterio}%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return MapearProducto(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar producto: {ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // Registrar nueva venta
        public static bool RegistrarVenta(List<DetalleVenta> detalles, decimal efectivoRecibido, int usuarioId, out int numeroTicket)
        {
            numeroTicket = 0;
            SQLiteConnection? connection = null;
            SQLiteTransaction? transaction = null;

            try
            {
                if (detalles == null || detalles.Count == 0)
                    throw new Exception("Debe agregar productos a la venta");

                connection = DatabaseManager.GetConnection();
                connection.Open();
                transaction = connection.BeginTransaction();

                // 1. Validar stock
                foreach (var detalle in detalles)
                {
                    int stockActual = ObtenerStock(connection, detalle.ProductoId);
                    if (stockActual < detalle.Cantidad)
                        throw new Exception($"Stock insuficiente: {detalle.NombreProducto} (Disponible: {stockActual})");
                }

                // 2. Calcular totales
                decimal subtotal = detalles.Sum(d => d.Subtotal);
                decimal total = subtotal;
                decimal vuelto = efectivoRecibido - total;

                if (efectivoRecibido < total)
                    throw new Exception($"Efectivo insuficiente. Falta: S/ {(total - efectivoRecibido):F2}");

                // 3. Obtener número de ticket
                numeroTicket = ObtenerSiguienteNumeroTicket(connection);

                // 4. Insertar venta
                string queryVenta = @"
                    INSERT INTO ventas (numero_ticket, usuario_id, subtotal, total, 
                                      efectivo_recibido, vuelto, estado) 
                    VALUES (@ticket, @usuario, @subtotal, @total, @efectivo, @vuelto, 'completada')";

                long ventaId;
                using (var cmd = new SQLiteCommand(queryVenta, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@ticket", numeroTicket);
                    cmd.Parameters.AddWithValue("@usuario", usuarioId);
                    cmd.Parameters.AddWithValue("@subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@efectivo", efectivoRecibido);
                    cmd.Parameters.AddWithValue("@vuelto", vuelto);
                    cmd.ExecuteNonQuery();
                    ventaId = connection.LastInsertRowId;
                }

                // 5. Insertar detalles y descontar stock
                foreach (var detalle in detalles)
                {
                    string queryDetalle = @"
                        INSERT INTO detalle_ventas (venta_id, producto_id, cantidad, precio_unitario, subtotal) 
                        VALUES (@venta_id, @producto_id, @cantidad, @precio, @subtotal)";

                    using (var cmd = new SQLiteCommand(queryDetalle, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@venta_id", ventaId);
                        cmd.Parameters.AddWithValue("@producto_id", detalle.ProductoId);
                        cmd.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                        cmd.Parameters.AddWithValue("@precio", detalle.PrecioUnitario);
                        cmd.Parameters.AddWithValue("@subtotal", detalle.Subtotal);
                        cmd.ExecuteNonQuery();
                    }

                    // Descontar stock
                    DescontarStock(connection, transaction, detalle.ProductoId, detalle.Cantidad);
                }

                // 6. Actualizar último ticket
                ActualizarUltimoTicket(connection, numeroTicket, transaction);

                // 7. Auditoría
                AuditoriaService.RegistrarAccion(usuarioId, "venta", "ventas", (int)ventaId, 
                    $"Ticket #{numeroTicket} - Total: S/ {total:F2}");

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show($"Error al registrar venta: {ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                connection?.Close();
            }
        }

        // Métodos auxiliares privados
        private static Producto MapearProducto(SQLiteDataReader reader)
        {
            return new Producto
            {
                Id = Convert.ToInt32(reader["id"]),
                CodigoBarras = reader["codigo_barras"].ToString(),
                Nombre = reader["nombre"].ToString(),
                Precio = Convert.ToDecimal(reader["precio"]),
                Stock = Convert.ToInt32(reader["stock"]),
                StockMinimo = Convert.ToInt32(reader["stock_minimo"]),
                Activo = Convert.ToBoolean(reader["activo"])
            };
        }

        private static int ObtenerStock(SQLiteConnection connection, int productoId)
        {
            string query = "SELECT stock FROM productos WHERE id = @id";
            using (var cmd = new SQLiteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@id", productoId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private static int ObtenerSiguienteNumeroTicket(SQLiteConnection connection)
        {
            string query = "SELECT valor FROM configuracion WHERE clave = 'ultimo_ticket'";
            using (var cmd = new SQLiteCommand(query, connection))
            {
                object? resultado = cmd.ExecuteScalar();
                int ultimoTicket = resultado != null ? Convert.ToInt32(resultado) : 0;
                return ultimoTicket + 1;
            }
        }

        private static void ActualizarUltimoTicket(SQLiteConnection connection, int numeroTicket, SQLiteTransaction transaction)
        {
            string query = "UPDATE configuracion SET valor = @valor WHERE clave = 'ultimo_ticket'";
            using (var cmd = new SQLiteCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@valor", numeroTicket.ToString());
                cmd.ExecuteNonQuery();
            }
        }

        private static void DescontarStock(SQLiteConnection connection, SQLiteTransaction transaction, int productoId, int cantidad)
        {
            string query = "UPDATE productos SET stock = stock - @cantidad WHERE id = @id";
            using (var cmd = new SQLiteCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                cmd.Parameters.AddWithValue("@id", productoId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}