using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Services;

namespace Users.API.Extensions.Endpoints
{
    public static class UsuariosEndpointsExtension
    {
        public static void MapUsuarioEndpoints(this WebApplication app)
        {
            // POST registrar un usuario nuevo
            app.MapPost("/api/users/register", async (RegistrarUsuarioDto req, UsuarioService service) =>
            {
                var usuario = await service.RegistrarAsync(req);
                return Results.Created($"/api/users/{usuario.Id}", usuario);
            })
            .WithTags("Users")
            .WithSummary("Registrar nuevo usuario")
            .Produces<UsuarioDto>(StatusCodes.Status201Created);

            // POST login con email y contraseña
            app.MapPost("/api/users/login", async (LoginDto req, UsuarioService service) =>
            {
                var usuario = await service.LoginAsync(req);
                return Results.Ok(usuario);
            })
            .WithTags("Users")
            .WithSummary("Autenticar usuario (email + password)")
            .Produces<LoginResponseDto>(StatusCodes.Status200OK);

            // GET by id lo usan Orders.API y Notifications.API
            app.MapGet("/api/users/{id}", async (string id, UsuarioService service) =>
            {
                // Si el id no tiene formato de Guid ese usuario no existe, tira USR-007
                if (!Guid.TryParse(id, out Guid guid))
                {
                    throw new NotFoundException(ErrorCodes.USR_007, "Usuario no encontrado.");
                }

                var usuario = await service.GetByIdAsync(guid);
                return Results.Ok(usuario);
            })
            .WithTags("Users")
            .WithSummary("Obtener usuario por ID")
            .Produces<UsuarioDto>(StatusCodes.Status200OK);
        }
    }
}