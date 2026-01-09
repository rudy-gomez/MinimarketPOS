using System.Data.SQLite;
using MinimarketPOS.Database;

namespace MinimarketPOS.Services
{
    public class AuditoriaService
    {
        // Registrar una acción en la auditoría
        public static void RegistrarAccion(int usuarioId, string accion, string tabla, int? registroId, string detalle)
        {
            try
            {
                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO auditoria (usuario_id, accion, tabla, registro_id, detalle) 
                        VALUES (@usuario_id, @accion, @tabla, @registro_id, @detalle)";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
                        cmd.Parameters.AddWithValue("@accion", accion);
                        cmd.Parameters.AddWithValue("@tabla", tabla ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@registro_id", registroId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@detalle", detalle);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // No mostrar error al usuario, solo registrar en consola
                Console.WriteLine($"Error al registrar auditoría: {ex.Message}");
            }
        }
    }
}