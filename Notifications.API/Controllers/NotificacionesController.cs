using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Services;
using Notifications.API.Swagger;

namespace Notifications.API.Controllers
{
    /// <summary>
    /// Endpoints de notificaciones. La lógica de negocio está en NotificacionService.
    /// </summary>
    [ApiController]
    [Route("api/notifications")]
    [Tags("Notifications")]
    [Produces("application/json")]
    public class NotificacionesController : ControllerBase
    {
        private readonly NotificacionService _service;

        public NotificacionesController(NotificacionService service)
        {
            _service = service;
        }

        /// <summary>Registrar y simular envío de notificación</summary>
        [HttpPost("send")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(NotificacionDto), StatusCodes.Status201Created)]
        [ErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.NTF_002)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.NTF_001)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.NTF_004)]
        public async Task<IActionResult> Enviar([FromBody, Required] EnviarNotificacionDto req, CancellationToken ct)
        {
            var notificacion = await _service.EnviarAsync(req, ct);
            return Created($"/api/notifications/{notificacion.UsuarioId}", notificacion);
        }

        /// <summary>Listar notificaciones de un usuario</summary>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(IEnumerable<NotificacionDto>), StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.NTF_003)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.NTF_004)]
        public async Task<IActionResult> GetByUsuario(string userId)
        {
            // Un userId que no es un Guid válido tampoco tiene notificaciones → NTF-003
            if (!Guid.TryParse(userId, out var id))
            {
                throw new NotFoundException(ErrorCodes.NTF_003, "No se encontraron notificaciones para el usuario.");
            }

            var notificaciones = await _service.GetByUsuarioAsync(id);
            return Ok(notificaciones);
        }
    }
}
