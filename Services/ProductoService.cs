using System.Data.SQLite;
using MinimarketPOS.Database;
using MinimarketPOS.Models;

namespace MinimarketPOS.Services
{
    public class ProductoService
    {
        // Obtener producto por ID
        public static Producto? ObtenerPorId(int id)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT * FROM productos WHERE id = @id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapearProducto(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener producto: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // Buscar producto por código de barras
        public static Producto? BuscarPorCodigo(string codigoBarras)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT * FROM productos 
                        WHERE codigo_barras = @codigo AND activo = 1";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigoBarras);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapearProducto(reader);
                            }
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

        // Listar todos los productos activos
        public static List<Producto> ListarTodos(bool incluirInactivos = false)
        {
            List<Producto> productos = new List<Producto>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = incluirInactivos
                        ? "SELECT * FROM productos ORDER BY nombre"
                        : "SELECT * FROM productos WHERE activo = 1 ORDER BY nombre";

                    using (var cmd = new SQLiteCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productos.Add(MapearProducto(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al listar productos: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return productos;
        }

        // Buscar productos por nombre o código
        public static List<Producto> Buscar(string criterio)
        {
            List<Producto> productos = new List<Producto>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT * FROM productos 
                        WHERE (nombre LIKE @criterio OR codigo_barras LIKE @criterio) 
                        AND activo = 1 
                        ORDER BY nombre";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@criterio", $"%{criterio}%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                productos.Add(MapearProducto(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar productos: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return productos;
        }

        // Agregar nuevo producto
        public static bool Agregar(Producto producto, int usuarioId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    // Verificar si el código ya existe
                    string checkQuery = "SELECT COUNT(*) FROM productos WHERE codigo_barras = @codigo";
                    using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@codigo", producto.CodigoBarras);
                        long count = (long)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Ya existe un producto con ese código de barras.",
                                          "Código duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    string query = @"
                        INSERT INTO productos (codigo_barras, nombre, precio, stock, stock_minimo, 
                                             fecha_vencimiento, categoria) 
                        VALUES (@codigo, @nombre, @precio, @stock, @stock_min, @vencimiento, @categoria)";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@codigo", producto.CodigoBarras);
                        cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                        cmd.Parameters.AddWithValue("@precio", producto.Precio);
                        cmd.Parameters.AddWithValue("@stock", producto.Stock);
                        cmd.Parameters.AddWithValue("@stock_min", producto.StockMinimo);
                        cmd.Parameters.AddWithValue("@vencimiento",
                            producto.FechaVencimiento.HasValue ?
                            (object)producto.FechaVencimiento.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@categoria", producto.Categoria ?? "");

                        cmd.ExecuteNonQuery();

                        // Registrar en auditoría
                        AuditoriaService.RegistrarAccion(usuarioId, "agregar_producto", "productos",
                            null, $"Producto agregado: {producto.Nombre}");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar producto: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Actualizar producto existente
        public static bool Actualizar(Producto producto, int usuarioId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        UPDATE productos 
                        SET nombre = @nombre, precio = @precio, stock = @stock, 
                            stock_minimo = @stock_min, fecha_vencimiento = @vencimiento, 
                            categoria = @categoria
                        WHERE id = @id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                        cmd.Parameters.AddWithValue("@precio", producto.Precio);
                        cmd.Parameters.AddWithValue("@stock", producto.Stock);
                        cmd.Parameters.AddWithValue("@stock_min", producto.StockMinimo);
                        cmd.Parameters.AddWithValue("@vencimiento",
                            producto.FechaVencimiento.HasValue ?
                            (object)producto.FechaVencimiento.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@categoria", producto.Categoria ?? "");
                        cmd.Parameters.AddWithValue("@id", producto.Id);

                        cmd.ExecuteNonQuery();

                        // Registrar en auditoría
                        AuditoriaService.RegistrarAccion(usuarioId, "editar_producto", "productos",
                            producto.Id, $"Producto actualizado: {producto.Nombre}");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar producto: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Desactivar producto (soft delete)
        public static bool Desactivar(int productoId, int usuarioId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = "UPDATE productos SET activo = 0 WHERE id = @id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", productoId);
                        cmd.ExecuteNonQuery();

                        // Registrar en auditoría
                        AuditoriaService.RegistrarAccion(usuarioId, "desactivar_producto", "productos",
                            productoId, "Producto desactivado");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desactivar producto: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Eliminar definitivamente (solo para admin)
        public static bool EliminarDefinitivamente(int productoId, int usuarioId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    // Verificar si tiene ventas asociadas
                    string checkQuery = "SELECT COUNT(*) FROM detalle_ventas WHERE producto_id = @id";
                    using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@id", productoId);
                        long count = (long)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("No se puede eliminar este producto porque tiene ventas asociadas.\n\nPuede desactivarlo en su lugar.",
                                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }

                    string query = "DELETE FROM productos WHERE id = @id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", productoId);
                        cmd.ExecuteNonQuery();

                        // Registrar en auditoría
                        AuditoriaService.RegistrarAccion(usuarioId, "eliminar_producto", "productos",
                            productoId, "Producto eliminado permanentemente");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar producto: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Obtener productos con stock bajo
        public static List<Producto> ObtenerConStockBajo()
        {
            List<Producto> productos = new List<Producto>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT * FROM productos 
                        WHERE stock <= stock_minimo AND activo = 1 
                        ORDER BY stock ASC";

                    using (var cmd = new SQLiteCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productos.Add(MapearProducto(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener productos: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return productos;
        }

        // Obtener productos próximos a vencer (< 30 días)
        public static List<Producto> ObtenerProximosAVencer()
        {
            List<Producto> productos = new List<Producto>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    DateTime hoy = DateTime.Now;
                    DateTime limite = hoy.AddDays(30);

                    string query = @"
                        SELECT * FROM productos 
                        WHERE fecha_vencimiento IS NOT NULL 
                        AND DATE(fecha_vencimiento) > DATE(@hoy)
                        AND DATE(fecha_vencimiento) <= DATE(@limite)
                        AND activo = 1 
                        ORDER BY fecha_vencimiento ASC";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@hoy", hoy.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@limite", limite.ToString("yyyy-MM-dd"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                productos.Add(MapearProducto(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener productos próximos a vencer: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return productos;
        }

        // Obtener productos vencidos
        public static List<Producto> ObtenerVencidos()
        {
            List<Producto> productos = new List<Producto>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    DateTime hoy = DateTime.Now;

                    string query = @"
                        SELECT * FROM productos 
                        WHERE fecha_vencimiento IS NOT NULL 
                        AND DATE(fecha_vencimiento) < DATE(@hoy)
                        AND activo = 1 
                        ORDER BY fecha_vencimiento ASC";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@hoy", hoy.ToString("yyyy-MM-dd"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                productos.Add(MapearProducto(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener productos vencidos: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return productos;
        }

        // Importar productos desde Excel
        public static int ImportarDesdeExcel(string rutaArchivo, int usuarioId)
        {
            int productosImportados = 0;

            try
            {
                using (var workbook = new ClosedXML.Excel.XLWorkbook(rutaArchivo))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Saltar encabezado

                    foreach (var row in rows)
                    {
                        try
                        {
                            var producto = new Producto
                            {
                                CodigoBarras = row.Cell(1).GetString(),
                                Nombre = row.Cell(2).GetString(),
                                Precio = row.Cell(3).GetValue<decimal>(),
                                Stock = row.Cell(4).GetValue<int>(),
                                StockMinimo = row.Cell(5).GetValue<int>(),
                                FechaVencimiento = row.Cell(6).IsEmpty() ? null : row.Cell(6).GetDateTime(),
                                Categoria = row.Cell(7).GetString()
                            };

                            if (Agregar(producto, usuarioId))
                            {
                                productosImportados++;
                            }
                        }
                        catch
                        {
                            continue; // Saltar filas con errores
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar productos: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return productosImportados;
        }

        // Mapear desde DataReader a objeto Producto
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
                FechaVencimiento = reader["fecha_vencimiento"] != DBNull.Value ?
                    Convert.ToDateTime(reader["fecha_vencimiento"]) : null,
                Categoria = reader["categoria"].ToString(),
                Activo = Convert.ToBoolean(reader["activo"]),
                FechaCreacion = Convert.ToDateTime(reader["fecha_creacion"])
            };
        }
    }
}