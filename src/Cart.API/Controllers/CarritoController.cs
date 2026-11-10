using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Services;
using Cart.API.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers
{
    /// <summary>
    /// Endpoints del carrito de compras.
    /// </summary>
    // el controller solo recibe el pedido y se lo pasa al servicio, la logica esta en CarritoService
    // las etiquetas ErrorExample son para q swagger muestre los errores q puede devolver cada endpoint
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

        /// <summary>Obtener carrito del usuario</summary>
        // GET /api/cart/{userId}
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_001)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005)]
        public async Task<IActionResult> GetCarrito(string userId)
        {
            var carrito = await _service.GetAsync(ParseUsuarioId(userId));
            return Ok(carrito);
        }

        /// <summary>Agregar producto al carrito</summary>
        // POST /api/cart/{userId}/items
        [HttpPost("{userId}/items")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.CRT_004)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_002)]
        [ErrorExample(StatusCodes.Status422UnprocessableEntity, ErrorCodes.CRT_003)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005)]
        public async Task<IActionResult> AgregarItem(string userId, [FromBody] AgregarItemDto request, CancellationToken ct)
        {
            var carrito = await _service.AgregarItemAsync(ParseUsuarioId(userId), request, ct);
            return Ok(carrito);
        }

        /// <summary>Actualizar cantidad de un item</summary>
        // PUT /api/cart/{userId}/items/{productId}
        [HttpPut("{userId}/items/{productId}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CarritoDto), StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.CRT_004)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_001)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_002)]
        [ErrorExample(StatusCodes.Status422UnprocessableEntity, ErrorCodes.CRT_003)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005)]
        public async Task<IActionResult> ActualizarCantidad(string userId, string productId, [FromBody] ActualizarCantidadDto request, CancellationToken ct)
        {
            var carrito = await _service.ActualizarCantidadAsync(ParseUsuarioId(userId), ParseProductoId(productId), request, ct);
            return Ok(carrito);
        }

        /// <summary>Quitar un producto del carrito</summary>
        // DELETE /api/cart/{userId}/items/{productId}
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_001)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005)]
        public async Task<IActionResult> QuitarItem(string userId, string productId)
        {
            await _service.QuitarItemAsync(ParseUsuarioId(userId), ParseProductoId(productId));
            return NoContent();
        }

        /// <summary>Vaciar carrito completo</summary>
        // DELETE /api/cart/{userId}
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.CRT_001)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.CRT_005)]
        public async Task<IActionResult> VaciarCarrito(string userId)
        {
            await _service.VaciarAsync(ParseUsuarioId(userId));
            return NoContent();
        }

        // si el userId no es un guid valido ese carrito no existe CRT-001
        private static Guid ParseUsuarioId(string userId)
        {
            if (Guid.TryParse(userId, out var guid))
            {
                return guid;
            }

            throw new NotFoundException(ErrorCodes.CRT_001, "Carrito no encontrado.");
        }

        // lo mismo con el productId, si no es guid el producto no existe CRT-002
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