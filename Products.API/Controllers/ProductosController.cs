using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Services;
using Products.API.Swagger;

namespace Products.API.Controllers
{
    /// <summary>
    /// ABM del catálogo de productos.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Tags("Products")]
    [Produces("application/json")]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoService _service;

        public ProductosController(ProductoService service) => _service = service;

        /// <summary>Lista los productos, con filtros opcionales.</summary>
        /// <param name="categoria">Filtra por categoría exacta (sin distinguir mayúsculas). Ej: Electrónica</param>
        /// <param name="nombre">Filtra por nombres que contengan este texto. Ej: notebook</param>
        /// <response code="200">Lista de productos (puede estar vacía).</response>
        /// <response code="500">PRD-005: error interno.</response>
        [HttpGet]
        [ProducesResponseType<IEnumerable<ProductoDto>>(StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status500InternalServerError, "PRD-005")]
        public async Task<IActionResult> GetAll([FromQuery] string? categoria, [FromQuery] string? nombre)
        {
            return Ok(await _service.GetAllAsync(categoria, nombre));
        }

        /// <summary>Obtiene un producto por su ID.</summary>
        /// <param name="id">ID del producto.</param>
        /// <response code="200">Producto encontrado.</response>
        /// <response code="404">PRD-001: el producto no existe.</response>
        /// <response code="500">PRD-005: error interno.</response>
        [HttpGet("{id}")]
        [ProducesResponseType<ProductoDto>(StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status404NotFound, "PRD-001")]
        [ErrorExample(StatusCodes.Status500InternalServerError, "PRD-005")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        /// <summary>Crea un producto nuevo.</summary>
        /// <response code="201">Producto creado.</response>
        /// <response code="400">PRD-002: faltan campos o tienen formato incorrecto.</response>
        /// <response code="409">PRD-003: ya existe un producto con ese nombre en la categoría.</response>
        /// <response code="500">PRD-005: error interno.</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType<ProductoDto>(StatusCodes.Status201Created)]
        [ErrorExample(StatusCodes.Status400BadRequest, "PRD-002")]
        [ErrorExample(StatusCodes.Status409Conflict, "PRD-003")]
        [ErrorExample(StatusCodes.Status500InternalServerError, "PRD-005")]
        public async Task<IActionResult> Create([FromBody] CrearProductoDto request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Actualiza un producto existente.</summary>
        /// <param name="id">ID del producto.</param>
        /// <param name="request">Datos nuevos del producto.</param>
        /// <response code="200">Producto actualizado.</response>
        /// <response code="400">PRD-002: faltan campos o tienen formato incorrecto.</response>
        /// <response code="404">PRD-001: el producto no existe.</response>
        /// <response code="500">PRD-005: error interno.</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType<ProductoDto>(StatusCodes.Status200OK)]
        [ErrorExample(StatusCodes.Status400BadRequest, "PRD-002")]
        [ErrorExample(StatusCodes.Status404NotFound, "PRD-001")]
        [ErrorExample(StatusCodes.Status500InternalServerError, "PRD-005")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CrearProductoDto request)
        {
            return Ok(await _service.UpdateAsync(id, request));
        }

        /// <summary>Elimina un producto.</summary>
        /// <remarks>No se puede eliminar si está en órdenes Pendiente o Confirmada (se consulta a Orders.API).</remarks>
        /// <param name="id">ID del producto.</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <response code="204">Producto eliminado.</response>
        /// <response code="404">PRD-001: el producto no existe.</response>
        /// <response code="409">PRD-004: el producto tiene órdenes activas.</response>
        /// <response code="500">PRD-005: error interno.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ErrorExample(StatusCodes.Status404NotFound, "PRD-001")]
        [ErrorExample(StatusCodes.Status409Conflict, "PRD-004")]
        [ErrorExample(StatusCodes.Status500InternalServerError, "PRD-005")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
