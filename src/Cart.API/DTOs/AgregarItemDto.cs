namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos para agregar un producto al carrito (POST /api/cart/{userId}/items).
    /// </summary>
    // Esto es lo que manda el cliente en el body. No lleva el usuario porque viene en la URL.
    public class AgregarItemDto
    {
        /// <summary>Id del producto en Products.API.</summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid ProductoId { get; set; }

        /// <summary>Cuántas unidades se agregan. Tiene que ser mayor a 0.</summary>
        /// <example>2</example>
        public int Cantidad { get; set; }
    }
}
