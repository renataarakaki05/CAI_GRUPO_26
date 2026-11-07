namespace Users.API.DTOs;

/// <summary>Datos para iniciar sesión.</summary>
/// <param name="Email" example="maria@email.com">Email del usuario.</param>
/// <param name="Password" example="MiPassword123!">Contraseña del usuario.</param>
public record LoginDto( // Request para loguearse
    string Email,
    string Password
);