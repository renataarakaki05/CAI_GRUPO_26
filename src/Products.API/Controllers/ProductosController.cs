using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Services;
using Products.API.Swagger;

namespace Products.API.Controllers
{
 
    // Endpoints de productos. 
    /// <summary>
    /// Endpoints de productos. La lógica de negocio está en ProductoService.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Tags("Products")]
    [Produces("application/json")]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoService _service;

        public ProductosController(ProductoService service)
        {
            _service = service;
        }

        /// <summary>Listar productos (filtro ?categoria= y ?nombre=)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductoDto>), StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005)]
        public async Task<IActionResult> GetAll(string? categoria, string? nombre)
        {
            var productos = await _service.GetAllAsync(categoria, nombre);
            return Ok(productos);
        }

        /// <summary>Obtener producto por ID</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.PRD_001)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005)]
        public async Task<IActionResult> GetById(string id)
        {
            var producto = await _service.GetByIdAsync(ParseId(id));
            return Ok(producto);
        }


        /// <summary>Crear nuevo producto</summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
        [ErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.PRD_002)]
        [ErrorExample(StatusCodes.Status409Conflict, ErrorCodes.PRD_003)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005)]
        public async Task<IActionResult> Create([FromBody, Required] CrearProductoDto req)
        {
            var producto = await _service.CreateAsync(req);
            return Created($"/api/products/{producto.Id}", producto);
        }


        /// <summary>Actualizar producto existente</summary>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status400BadRequest, ErrorCodes.PRD_002)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.PRD_001)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005)]
        public async Task<IActionResult> Update(string id, [FromBody, Required] CrearProductoDto req)
        {
            var producto = await _service.UpdateAsync(ParseId(id), req);
            return Ok(producto);
        }


        /// <summary>Eliminar producto</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ErrorExample(StatusCodes.Status404NotFound, ErrorCodes.PRD_001)]
        [ErrorExample(StatusCodes.Status409Conflict, ErrorCodes.PRD_004)]
        [ErrorExample(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005)]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            await _service.DeleteAsync(ParseId(id), ct);
            return NoContent();
        }


        private static Guid ParseId(string id)
        {
            if (Guid.TryParse(id, out var guid))
            {
                return guid;
            }

            throw new NotFoundException(ErrorCodes.PRD_001, "Producto no encontrado.");
        }
    }
}
