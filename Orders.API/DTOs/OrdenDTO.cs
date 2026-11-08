namespace Orders.API.DTOs;

public record OrdenDto( // Response con los datos de la orden
    Guid Id,
    Guid UsuarioId,
    List<OrdenItemDto> Items,
    decimal Total,
    string Estado,
    DateTime FechaCreacion
);

public record OrdenItemDto( // Cada producto de la response
    Guid ProductoId,
    int Cantidad,
    decimal PrecioUnitario
);