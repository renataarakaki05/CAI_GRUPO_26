namespace Users.API.DTOs;

/// <summary>Datos para registrar un usuario nuevo.</summary>
/// <param name="Nombre" example="María">Nombre del usuario. Obligatorio.</param>
/// <param name="Apellido" example="González">Apellido del usuario. Obligatorio.</param>
/// <param name="Email" example="maria@email.com">Email del usuario. Obligatorio y no se puede repetir.</param>
/// <param name="Password" example="MiPassword123!">Contraseña. Mínimo 6 caracteres.</param>
public record RegistrarUsuarioDto( // Request para registrar un usuario
    string Nombre,
    string Apellido,
    string Email,
    string Password
);