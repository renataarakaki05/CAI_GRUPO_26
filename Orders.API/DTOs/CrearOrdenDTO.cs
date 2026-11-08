namespace Orders.API.DTOs;

public record CrearOrdenDto( // Request para crear una orden
    Guid UsuarioId,
    List<CrearOrdenItemDto> Items
);

public record CrearOrdenItemDto( // Cada producto del request
    Guid ProductoId,
    int Cantidad
);