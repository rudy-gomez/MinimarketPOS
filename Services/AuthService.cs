using System.Data.SQLite;
using MinimarketPOS.Database;
using MinimarketPOS.Models;

namespace MinimarketPOS.Services
{
    public class AuthService
    {
        // Validar usuario y contraseña
        public static Usuario? Login(string username, string password)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT id, username, nombre, rol, password_hash, activo 
                        FROM usuarios 
                        WHERE username = @username AND activo = 1";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["password_hash"].ToString();

                                // Verificar contraseña con BCrypt
                                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                                {
                                    // Crear objeto Usuario
                                    Usuario usuario = new Usuario
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Username = reader["username"].ToString(),
                                        Nombre = reader["nombre"].ToString(),
                                        Rol = reader["rol"].ToString(),
                                        Activo = Convert.ToBoolean(reader["activo"])
                                    };

                                    // Registrar login en auditoría
                                    AuditoriaService.RegistrarAccion(
                                        usuario.Id,
                                        "login",
                                        "usuarios",
                                        usuario.Id,
                                        $"Inicio de sesión exitoso - Usuario: {username}"
                                    );

                                    return usuario;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al validar usuario: {ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }

            return null;
        }

        // Cambiar contraseña
        public static bool CambiarPassword(int usuarioId, string passwordActual, string passwordNueva)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    // Verificar password actual
                    string queryVerificar = "SELECT password_hash FROM usuarios WHERE id = @id";
                    using (var cmd = new SQLiteCommand(queryVerificar, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        string storedHash = cmd.ExecuteScalar()?.ToString();

                        if (storedHash == null || !BCrypt.Net.BCrypt.Verify(passwordActual, storedHash))
                        {
                            MessageBox.Show("La contraseña actual es incorrecta",
                                          "Error",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Actualizar con nueva contraseña
                    string nuevoHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);
                    string queryActualizar = "UPDATE usuarios SET password_hash = @hash WHERE id = @id";

                    using (var cmd = new SQLiteCommand(queryActualizar, connection))
                    {
                        cmd.Parameters.AddWithValue("@hash", nuevoHash);
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        cmd.ExecuteNonQuery();
                    }

                    // Registrar en auditoría
                    AuditoriaService.RegistrarAccion(
                        usuarioId,
                        "cambio_password",
                        "usuarios",
                        usuarioId,
                        "Cambio de contraseña exitoso"
                    );

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar contraseña: {ex.Message}",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                return false;
            }
        }
    }
}