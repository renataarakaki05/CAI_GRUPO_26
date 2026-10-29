using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs
{
    /// <summary>
    /// Datos para registrar y simular el envío de una notificación (POST).
    /// </summary>
    public class EnviarNotificacionDto
    {
        /// <summary>ID del usuario destinatario. Debe existir en Users.API.</summary>
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        [Required(ErrorMessage = "El usuarioId es requerido")]
        public Guid? UsuarioId { get; set; }

        /// <summary>Texto de la notificación. Requerido, máximo 500 caracteres.</summary>
        /// <example>Su orden #f1e2d3c4 fue confirmada.</example>
        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres")]
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>Canal de envío: Email, Push o SMS.</summary>
        /// <example>Email</example>
        [Required(ErrorMessage = "El tipo es requerido")]
        [RegularExpression("^(?i)(Email|Push|SMS)$", ErrorMessage = "El tipo debe ser Email, Push o SMS")]
        public string Tipo { get; set; } = string.Empty;
    }
}
