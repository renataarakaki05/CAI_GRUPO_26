using Cart.API.Models;

namespace Cart.API.DTOs
{
    /// <summary>
    /// Carrito devuelto por la API.
    /// </summary>
    // Es lo que ve el cliente cuando pide o modifica el carrito.
    public class CarritoDto
    {
        /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
        public Guid UsuarioId { get; set; }

        public List<CarritoItemDto> Items { get; set; } = new List<CarritoItemDto>();

        /// <example>2024-03-10T10:50:00Z</example>
        public DateTime FechaActualizacion { get; set; }

        // Pasa el carrito del modelo al formato de respuesta, item por item
        public static CarritoDto FromEntity(Carrito carrito)
        {
            var dto = new CarritoDto();
            dto.UsuarioId = carrito.UsuarioId;
            dto.FechaActualizacion = carrito.FechaActualizacion;

            foreach (var item in carrito.Items)
            {
                dto.Items.Add(CarritoItemDto.FromEntity(item));
            }

            return dto;
        }
    }
}   
