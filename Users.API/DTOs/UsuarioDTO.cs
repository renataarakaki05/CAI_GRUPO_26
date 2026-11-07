namespace Users.API.DTOs;

/// <summary>Usuario que devuelve la API. No tiene la contraseña a propósito.</summary>
/// <param name="Id" example="a1b2c3d4-0000-0000-0000-111122223333">Id del usuario.</param>
/// <param name="Nombre" example="María">Nombre del usuario.</param>
/// <param name="Apellido" example="González">Apellido del usuario.</param>
/// <param name="Email" example="maria@email.com">Email del usuario.</param>
/// <param name="FechaRegistro" example="2024-03-10T09:00:00Z">Fecha de registro (UTC).</param>
/// <param name="Activo" example="true">false si la cuenta está bloqueada.</param>
public record UsuarioDto( // Response con los datos del usuario
    Guid Id,
    string Nombre,
    string Apellido,
    string Email,
    DateTime FechaRegistro,
    bool Activo
);

/// <summary>Respuesta del login.</summary>
/// <param name="Id" example="a1b2c3d4-0000-0000-0000-111122223333">Id del usuario.</param>
/// <param name="Nombre" example="María">Nombre del usuario.</param>
/// <param name="Apellido" example="González">Apellido del usuario.</param>
/// <param name="Email" example="maria@email.com">Email del usuario.</param>
public record LoginResponseDto( // Response del login
    Guid Id,
    string Nombre,
    string Apellido,
    string Email
);