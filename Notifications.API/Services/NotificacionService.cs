using System.ComponentModel.DataAnnotations;
using Notifications.API.Clients;
using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;
using ValidationException = Notifications.API.Exceptions.ValidationException;

namespace Notifications.API.Services
{
    // Lógica de negocio de notificaciones. Los errores se lanzan como excepciones
    // de dominio y los IExceptionHandler arman la respuesta.
    public class NotificacionService
    {
        private static readonly string[] TiposValidos = ["Email", "Push", "SMS"];

        private readonly NotificacionRepository _repo;
        private readonly UsersClient _users;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(NotificacionRepository repo, UsersClient users, ILogger<NotificacionService> logger)
        {
            _repo = repo;
            _users = users;
            _logger = logger;
        }

        public async Task<NotificacionDto> EnviarAsync(EnviarNotificacionDto request, CancellationToken ct)
        {
            Validar(request);
            var usuarioId = request.UsuarioId!.Value;

            if (!await _users.ExisteUsuarioAsync(usuarioId, ct))
                throw new NotFoundException(ErrorCodes.NTF_001, "El usuario destinatario no fue encontrado.");

            var notificacion = new Notificacion
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Mensaje = request.Mensaje.Trim(),
                // Se guarda con la forma canónica aunque llegue "email" o "sms"
                Tipo = TiposValidos.First(t => t.Equals(request.Tipo.Trim(), StringComparison.OrdinalIgnoreCase)),
                FechaEnvio = DateTime.UtcNow
            };

            // El envío es simulado: no hay proveedor real de Email/Push/SMS
            _logger.LogInformation("Simulando envío por {Tipo} al usuario {UsuarioId}", notificacion.Tipo, usuarioId);
            notificacion.Estado = "Enviada";

            await _repo.CreateAsync(notificacion);
            _logger.LogInformation("Notificación {NotificacionId} registrada", notificacion.Id);

            return NotificacionDto.FromEntity(notificacion);
        }

        public async Task<IEnumerable<NotificacionDto>> GetByUsuarioAsync(Guid usuarioId)
        {
            var notificaciones = (await _repo.GetByUsuarioAsync(usuarioId)).ToList();

            if (notificaciones.Count == 0)
                throw new NotFoundException(ErrorCodes.NTF_003, "No se encontraron notificaciones para el usuario.");

            return notificaciones.Select(NotificacionDto.FromEntity);
        }

        // Valida las Data Annotations del DTO; todos los problemas van separados por punto y coma (NTF-002)
        private static void Validar(EnviarNotificacionDto request)
        {
            var resultados = new List<ValidationResult>();
            if (Validator.TryValidateObject(request, new ValidationContext(request), resultados, validateAllProperties: true))
                return;

            var errores = resultados.Select(r => r.ErrorMessage);
            throw new ValidationException(ErrorCodes.NTF_002,
                $"Los datos de la notificación son inválidos: {string.Join("; ", errores)}.");
        }
    }
}
