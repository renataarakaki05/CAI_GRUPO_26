namespace Orders.API.DTOs;

public record ActualizarEstadoDto( // Request para cambiar el estado
    string Estado
);

public record EstadoActualizadoDto( // Response del cambio de estado
    Guid Id,
    string Estado,
    DateTime FechaActualizacion
);