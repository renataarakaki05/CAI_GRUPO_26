namespace Users.API.DTOs;

public record LoginDto( // Request para loguearse 
    string Email,
    string Password
);