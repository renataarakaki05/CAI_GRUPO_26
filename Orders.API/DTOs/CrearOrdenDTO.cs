namespace Orders.API.DTOs;

/// <summary>Datos para crear una orden nueva.</summary>
/// <param name="UsuarioId" example="a1b2c3d4-0000-0000-0000-111122223333">Id del usuario que hace la compra.</param>
/// <param name="Items">Productos de la orden. Tiene que haber al menos uno.</param>
public record CrearOrdenDto( // Request para crear una orden
    Guid UsuarioId,
    List<CrearOrdenItemDto> Items
);

/// <summary>Producto que se quiere comprar.</summary>
/// <param name="ProductoId" example="3fa85f64-5717-4562-b3fc-2c963f66afa6">Id del producto.</param>
/// <param name="Cantidad" example="2">Cantidad. Tiene que ser mayor a 0.</param>
public record CrearOrdenItemDto( // Cada producto del request
    Guid ProductoId,
    int Cantidad
);