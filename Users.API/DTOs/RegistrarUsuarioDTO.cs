namespace Users.API.DTOs;
public record RegistrarUsuarioDto( // Request para registrar un usuario
    string Nombre,
    string Apellido,
    string Email,
    string Password
);