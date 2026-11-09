using Cart.API.Models;

namespace Cart.API.DTOs
{
    /// <summary>
    /// Un producto del carrito, tal como lo devuelve la API.
    /// </summary>
    public class CarritoItemDto
    {
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid ProductoId { get; set; }

        /// <example>2</example>
        public int Cantidad { get; set; }

        // Pasa un item del modelo al formato de respuesta
        public static CarritoItemDto FromEntity(CarritoItem item)
        {
            var dto = new CarritoItemDto();
            dto.ProductoId = item.ProductoId;
            dto.Cantidad = item.Cantidad;
            return dto;
        }
    }
}
