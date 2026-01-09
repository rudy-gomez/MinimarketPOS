using System.Data.SQLite;
using MinimarketPOS.Database;
using MinimarketPOS.Models;

namespace MinimarketPOS.Services
{
    public class UsuarioService
    {
        // Listar todos los usuarios
        public static List<Usuario> ListarTodos()
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT * FROM usuarios ORDER BY nombre";

                    using (var cmd = new SQLiteCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(new Usuario
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"].ToString(),
                                Nombre = reader["nombre"].ToString(),
                                Rol = reader["rol"].ToString(),
                                Activo = Convert.ToBoolean(reader["activo"]),
                                FechaCreacion = Convert.ToDateTime(reader["fecha_creacion"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al listar usuarios: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return usuarios;
        }

        // Crear nuevo usuario
        public static bool Crear(string username, string password, string nombre, string rol, int adminId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    // Verificar si username ya existe
                    string queryCheck = "SELECT COUNT(*) FROM usuarios WHERE username = @username";
                    using (var cmd = new SQLiteCommand(queryCheck, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        long count = (long)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("El nombre de usuario ya existe",
                                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Crear usuario
                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    string query = @"
                INSERT INTO usuarios (username, password_hash, nombre, rol) 
                VALUES (@username, @password, @nombre, @rol)";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", passwordHash);
                        cmd.Parameters.AddWithValue("@nombre", nombre);
                        cmd.Parameters.AddWithValue("@rol", rol);
                        cmd.ExecuteNonQuery();

                        // Registrar en auditoría
                        AuditoriaService.RegistrarAccion(adminId, "crear_usuario", "usuarios",
                            null, $"Usuario creado: {username} - {nombre}");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear usuario: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Restablecer contraseña de un usuario (solo admin)
        public static bool RestablecerPassword(int usuarioId, string nuevaPassword, int adminId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
                    string query = "UPDATE usuarios SET password_hash = @password WHERE id = @id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@password", passwordHash);
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        cmd.ExecuteNonQuery();

                        // Registrar en auditoría
                        AuditoriaService.RegistrarAccion(adminId, "restablecer_password", "usuarios",
                            usuarioId, $"Contraseña restablecida para usuario ID: {usuarioId}");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restablecer contraseña: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Cambiar estado activo/inactivo
        public static bool CambiarEstado(int usuarioId, bool activo, int adminId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = "UPDATE usuarios SET activo = @activo WHERE id = @id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@activo", activo);
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        cmd.ExecuteNonQuery();

                        // Registrar en auditoría
                        AuditoriaService.RegistrarAccion(adminId, "cambio_estado_usuario", "usuarios",
                            usuarioId, $"Usuario {(activo ? "activado" : "desactivado")} - ID: {usuarioId}");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar estado: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        // Eliminar usuario
        public static bool Eliminar(int usuarioId, int adminId)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    // BORRADO FÍSICO - DELETE real
                    string query = "DELETE FROM usuarios WHERE id = @id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioId);
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            // Registrar en auditoría ANTES de que se borre
                            AuditoriaService.RegistrarAccion(adminId, "eliminar_usuario", "usuarios",
                                usuarioId, $"Usuario ELIMINADO permanentemente - ID: {usuarioId}");
                        }

                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar usuario: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}