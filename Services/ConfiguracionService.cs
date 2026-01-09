using System.Data.SQLite;
using MinimarketPOS.Database;

namespace MinimarketPOS.Services
{
    public class ConfiguracionService
    {
        // Obtener valor de configuración
        public static string? ObtenerValor(string clave)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT valor FROM configuracion WHERE clave = @clave";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@clave", clave);
                        object? resultado = cmd.ExecuteScalar();
                        return resultado?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener configuración: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // Actualizar valor de configuración
        public static bool ActualizarValor(string clave, string valor, int usuarioId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        INSERT OR REPLACE INTO configuracion (clave, valor) 
                        VALUES (@clave, @valor)";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@clave", clave);
                        cmd.Parameters.AddWithValue("@valor", valor);
                        cmd.ExecuteNonQuery();

                        // Registrar en auditoría
                        AuditoriaService.RegistrarAccion(usuarioId, "actualizar_config", "configuracion",
                            null, $"Configuración actualizada: {clave} = {valor}");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar configuración: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Obtener toda la configuración
        public static Dictionary<string, string> ObtenerTodo()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT clave, valor FROM configuracion";

                    using (var cmd = new SQLiteCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            config[reader["clave"].ToString()] = reader["valor"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener configuración: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return config;
        }
    }
}