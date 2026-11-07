namespace Cart.API.DTOs
{
    /// <summary>
    /// Datos para cambiar la cantidad de un producto del carrito (PUT /api/cart/{userId}/items/{productId}).
    /// </summary>
    // Solo trae la cantidad nueva: el usuario y el producto vienen en la URL.
    public class ActualizarCantidadDto
    {
        /// <summary>Nueva cantidad. Tiene que ser mayor a 0.</summary>
        /// <example>4</example>
        public int Cantidad { get; set; }
    }
}
