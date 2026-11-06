namespace Users.API.DTOs;

public record UsuarioDto( // Response con los datos del usuario

    Guid Id,
    string Nombre,
    string Apellido,
    string Email,
    DateTime FechaRegistro,
    bool Activo
);
public record LoginResponseDto( // Response del login
    Guid Id,
    string Nombre,
    string Apellido,
    string Email
);

