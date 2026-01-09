using System.Data.SQLite;

namespace MinimarketPOS.Database
{
    public class DatabaseManager
    {
        private static string connectionString = "Data Source=minimarket.db;Version=3;";

        // Obtener conexión
        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        // Inicializar base de datos (crear archivo y tablas)
        public static void Initialize()
        {
            // Si no existe el archivo .db, se crea automáticamente
            if (!File.Exists("minimarket.db"))
            {
                SQLiteConnection.CreateFile("minimarket.db");
                Console.WriteLine("Base de datos creada: minimarket.db");
            }

            // Crear las tablas
            DatabaseSetup.CrearTablas();
            DatabaseSetup.InsertarDatosIniciales();
        }

        // Verificar si la BD está vacía (primera ejecución)
        public static bool EsPrimeraEjecucion()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM usuarios";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    long count = (long)cmd.ExecuteScalar();
                    return count == 0;
                }
            }
        }
    }
}