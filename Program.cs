using MinimarketPOS.Database;
using MinimarketPOS.Forms;

namespace MinimarketPOS
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                // Inicializar base de datos
                DatabaseManager.Initialize();

                // Mostrar mensaje solo en primera ejecución
                bool esPrimeraVez = DatabaseManager.EsPrimeraEjecucion();

                if (esPrimeraVez)
                {
                    MessageBox.Show(
                        "¡Bienvenido al Sistema Minimarket POS!\n\n" +
                        "Usuario por defecto:\n" +
                        "Usuario: admin\n" +
                        "Contraseña: admin123\n\n" +
                        "Por favor cambie la contraseña después del primer login.",
                        "Primera Ejecución",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                // Iniciar con FormLogin
                Application.Run(new FormLogin());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al iniciar la aplicación:\n{ex.Message}",
                    "Error Fatal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}