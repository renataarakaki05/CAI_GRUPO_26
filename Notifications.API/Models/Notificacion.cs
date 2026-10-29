namespace Notifications.API.Models
{
    public class Notificacion //la información de una notificación enviada
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UsuarioId { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        // Email | Push | SMS
        public string Tipo { get; set; } = string.Empty;

        // Pendiente | Enviada | Fallida
        public string Estado { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    }
}
