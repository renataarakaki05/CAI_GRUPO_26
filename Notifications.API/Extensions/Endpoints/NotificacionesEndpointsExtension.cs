using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Services;
using Notifications.API.Swagger;

namespace Notifications.API.Extensions.Endpoints
{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad
    public static class NotificacionesEndpointsExtension
    {
        public static void MapNotificacionEndpoints(this WebApplication app)
        {
            // POST -- registra la notificación y simula el envío
            app.MapPost("/api/notifications/send", async (EnviarNotificacionDto req, NotificacionService service, CancellationToken ct) =>
            {
                var notificacion = await service.EnviarAsync(req, ct);
                return Results.Created($"/api/notifications/{notificacion.UsuarioId}", notificacion);
            })
            .WithTags("Notifications")
            .WithSummary("Registrar y simular envío de notificación")
            .Produces<NotificacionDto>(StatusCodes.Status201Created)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status400BadRequest, ErrorCodes.NTF_002))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.NTF_001))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.NTF_004));

            // GET -- notificaciones de un usuario
            app.MapGet("/api/notifications/{userId}", async (string userId, NotificacionService service) =>
            {
                // Un userId que no es un Guid válido tampoco tiene notificaciones → NTF-003
                if (!Guid.TryParse(userId, out var id))
                    throw new NotFoundException(ErrorCodes.NTF_003, "No se encontraron notificaciones para el usuario.");

                var notificaciones = await service.GetByUsuarioAsync(id);
                return Results.Ok(notificaciones);
            })
            .WithTags("Notifications")
            .WithSummary("Listar notificaciones de un usuario")
            .Produces<IEnumerable<NotificacionDto>>(StatusCodes.Status200OK)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.NTF_003))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.NTF_004));
        }
    }
}
