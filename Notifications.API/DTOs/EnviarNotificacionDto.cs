using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs
{
    
    //datos para registrar y simular el envío de una notificación (POST).
    
    public class EnviarNotificacionDto
    {
        //ID del usuario destinatario. tiene que existir en Users.API.
        
        [Required(ErrorMessage = "El usuarioId es requerido")]
        public Guid? UsuarioId { get; set; }

        
        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres")]
        public string Mensaje { get; set; } = string.Empty;

      
        [Required(ErrorMessage = "El tipo es requerido")]
        [RegularExpression("^(?i)(Email|Push|SMS)$", ErrorMessage = "El tipo debe ser Email, Push o SMS")]
        public string Tipo { get; set; } = string.Empty;
    }
}
