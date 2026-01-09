namespace MinimarketPOS.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Constructor vacío
        public Usuario() { }

        // Constructor con parámetros
        public Usuario(int id, string username, string nombre, string rol)
        {
            Id = id;
            Username = username;
            Nombre = nombre;
            Rol = rol;
            Activo = true;
        }

        // Método helper para verificar si es admin
        public bool EsAdmin()
        {
            return Rol.ToLower() == "admin";
        }
    }
}