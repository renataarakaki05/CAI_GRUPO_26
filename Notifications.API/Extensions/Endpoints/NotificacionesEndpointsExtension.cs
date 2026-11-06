using System.ComponentModel.DataAnnotations;
using Notifications.API.Clients;
using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;
using Notifications.API.Swagger;
using ValidationException = Notifications.API.Exceptions.ValidationException;

namespace Notifications.API.Extensions.Endpoints
{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad
    public static class NotificacionesEndpointsExtension
    {
        private static readonly string[] TiposValidos = { "Email", "Push", "SMS" };

        public static void MapNotificacionEndpoints(this WebApplication app)
        {
            // POST -- registra la notificación y simula el envío
            app.MapPost("/api/notifications/send", async (EnviarNotificacionDto req, NotificacionRepository repo, UsersClient users, ILogger<Program> logger, CancellationToken ct) =>
            {
                Validar(req);
                var usuarioId = req.UsuarioId!.Value;

                if (!await users.ExisteUsuarioAsync(usuarioId, ct))
                {
                    throw new NotFoundException(ErrorCodes.NTF_001, "El usuario destinatario no fue encontrado.");
                }

                // Se guarda con la forma canónica aunque llegue "email" o "sms"
                var tipo = req.Tipo.Trim();
                foreach (var tipoValido in TiposValidos)
                {
                    if (tipoValido.Equals(tipo, StringComparison.OrdinalIgnoreCase))
                    {
                        tipo = tipoValido;
                        break;
                    }
                }

                var notificacion = new Notificacion();
                notificacion.Id = Guid.NewGuid();
                notificacion.UsuarioId = usuarioId;
                notificacion.Mensaje = req.Mensaje.Trim();
                notificacion.Tipo = tipo;
                notificacion.FechaEnvio = DateTime.UtcNow;

                // El envío es simulado: no hay proveedor real de Email/Push/SMS
                logger.LogInformation("Simulando envío por {Tipo} al usuario {UsuarioId}", notificacion.Tipo, usuarioId);
                notificacion.Estado = "Enviada";

                await repo.CreateAsync(notificacion);
                logger.LogInformation("Notificación {NotificacionId} registrada", notificacion.Id);

                return Results.Created($"/api/notifications/{notificacion.UsuarioId}", NotificacionDto.FromEntity(notificacion));
            })
            .WithTags("Notifications")
            .WithSummary("Registrar y simular envío de notificación")
            .Produces<NotificacionDto>(StatusCodes.Status201Created)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status400BadRequest, ErrorCodes.NTF_002))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.NTF_001))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.NTF_004));

            // GET -- notificaciones de un usuario
            app.MapGet("/api/notifications/{userId}", async (string userId, NotificacionRepository repo) =>
            {
                // Un userId que no es un Guid válido tampoco tiene notificaciones → NTF-003
                if (!Guid.TryParse(userId, out var id))
                {
                    throw new NotFoundException(ErrorCodes.NTF_003, "No se encontraron notificaciones para el usuario.");
                }

                var notificaciones = await repo.GetByUsuarioAsync(id);

                var resultado = new List<NotificacionDto>();
                foreach (var notificacion in notificaciones)
                {
                    resultado.Add(NotificacionDto.FromEntity(notificacion));
                }

                if (resultado.Count == 0)
                {
                    throw new NotFoundException(ErrorCodes.NTF_003, "No se encontraron notificaciones para el usuario.");
                }

                return Results.Ok(resultado);
            })
            .WithTags("Notifications")
            .WithSummary("Listar notificaciones de un usuario")
            .Produces<IEnumerable<NotificacionDto>>(StatusCodes.Status200OK)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.NTF_003))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.NTF_004));
        }

        // Valida las Data Annotations del DTO; todos los problemas van separados por punto y coma (NTF-002)
        private static void Validar(EnviarNotificacionDto req)
        {
            var resultados = new List<ValidationResult>();
            if (Validator.TryValidateObject(req, new ValidationContext(req), resultados, validateAllProperties: true))
            {
                return;
            }

            var errores = new List<string?>();
            foreach (var resultado in resultados)
            {
                errores.Add(resultado.ErrorMessage);
            }

            throw new ValidationException(ErrorCodes.NTF_002,
                $"Los datos de la notificación son inválidos: {string.Join("; ", errores)}.");
        }
    }
}
