using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers
{
    
    // Endpoints del carrito de compras.
    
    // el controller solo recibe el pedido y se lo pasa al servicio, la logica esta en CarritoService
    [ApiController]
    [Route("api/cart")]
    [Tags("Cart")]
    [Produces("application/json")]
    public class CarritoController : ControllerBase
    {
        private readonly CarritoService _service;

        public CarritoController(CarritoService service)
        {
            _service = service;
        }

       
        // GET /api/cart/{userId}
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCarrito(string userId)
        {
            var carrito = await _service.GetAsync(ParseUsuarioId(userId));
            return Ok(carrito);
        }

       
        // POST /api/cart/{userId}/items
        [HttpPost("{userId}/items")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AgregarItem(string userId, [FromBody] AgregarItemDto request, CancellationToken ct)
        {
            var carrito = await _service.AgregarItemAsync(ParseUsuarioId(userId), request, ct);
            return Ok(carrito);
        }

       
        // PUT /api/cart/{userId}/items/{productId}
        [HttpPut("{userId}/items/{productId}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ActualizarCantidad(string userId, string productId, [FromBody] ActualizarCantidadDto request, CancellationToken ct)
        {
            var carrito = await _service.ActualizarCantidadAsync(ParseUsuarioId(userId), ParseProductoId(productId), request, ct);
            return Ok(carrito);
        }

       
        // DELETE /api/cart/{userId}/items/{productId}
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> QuitarItem(string userId, string productId)
        {
            await _service.QuitarItemAsync(ParseUsuarioId(userId), ParseProductoId(productId));
            return NoContent();
        }

        
        // DELETE /api/cart/{userId}
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> VaciarCarrito(string userId)
        {
            await _service.VaciarAsync(ParseUsuarioId(userId));
            return NoContent();
        }

        // si el userId no es un guid valido ese carrito no existe, CRT-001
        private static Guid ParseUsuarioId(string userId)
        {
            if (Guid.TryParse(userId, out var guid))
            {
                return guid;
            }

            throw new NotFoundException(ErrorCodes.CRT_001, "Carrito no encontrado.");
        }

        // lo mismo con el productId, si no es guid el producto no existe, CRT-002
        private static Guid ParseProductoId(string productId)
        {
            if (Guid.TryParse(productId, out var guid))
            {
                return guid;
            }

            throw new NotFoundException(ErrorCodes.CRT_002, "Producto no encontrado.");
        }
    }
}
