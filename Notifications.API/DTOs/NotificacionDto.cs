using Notifications.API.Models;

namespace Notifications.API.DTOs
{
    /// notif que devuekve por la API.
    public class NotificacionDto
    {
        public Guid Id { get; set; }

        public Guid UsuarioId { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; }

        public static NotificacionDto FromEntity(Notificacion n)
        {
            var dto = new NotificacionDto();
            dto.Id = n.Id;
            dto.UsuarioId = n.UsuarioId;
            dto.Mensaje = n.Mensaje;
            dto.Tipo = n.Tipo;
            dto.Estado = n.Estado;
            dto.FechaEnvio = n.FechaEnvio;
            return dto;
        }
    }
}
