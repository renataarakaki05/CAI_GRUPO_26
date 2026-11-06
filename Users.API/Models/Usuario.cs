namespace Users.API.Models
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public int IntentosFallidos { get; set; }
    }
}
