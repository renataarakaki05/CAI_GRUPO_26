namespace Orders.API.DTOs;

/// <summary>Orden que devuelve la API.</summary>
/// <param name="Id" example="f1e2d3c4-0000-0000-0000-aabbccddeeff">Id de la orden.</param>
/// <param name="UsuarioId" example="a1b2c3d4-0000-0000-0000-111122223333">Id del usuario que hizo la compra.</param>
/// <param name="Items">Productos de la orden.</param>
/// <param name="Total" example="3000.00">Total de la orden.</param>
/// <param name="Estado" example="Pendiente">Pendiente, Confirmada, Enviada, Entregada o Cancelada.</param>
/// <param name="FechaCreacion" example="2024-03-10T11:00:00Z">Fecha de creación (UTC).</param>
public record OrdenDto( // Response con los datos de la orden
    Guid Id,
    Guid UsuarioId,
    List<OrdenItemDto> Items,
    decimal Total,
    string Estado,
    DateTime FechaCreacion
);

/// <summary>Producto de una orden.</summary>
/// <param name="ProductoId" example="3fa85f64-5717-4562-b3fc-2c963f66afa6">Id del producto.</param>
/// <param name="Cantidad" example="2">Cantidad comprada.</param>
/// <param name="PrecioUnitario" example="1500.00">Precio del producto al momento de crear la orden.</param>
public record OrdenItemDto( // Cada producto de la response
    Guid ProductoId,
    int Cantidad,
    decimal PrecioUnitario
);