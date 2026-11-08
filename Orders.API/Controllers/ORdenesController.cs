using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    // Endpoints de ordenes. La logica de negocio esta en OrdenService
    [ApiController]
    [Route("api/orders")]
    [Tags("Orders")]
    [Produces("application/json")]
    public class OrdenesController : ControllerBase
    {
        private readonly OrdenService _service;

        public OrdenesController(OrdenService service)
        {
            _service = service;
        }

        // GET 
        /// <summary>Listar órdenes (filtro opcional ?usuarioId=)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<OrdenDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(string? usuarioId)
        {
            // Sin filtro: todas las órdenes
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return Ok(await _service.GetAllAsync(null));
            }

            // Si el id no tiene formato de Guid, ese usuario no tiene órdenes
            if (!Guid.TryParse(usuarioId, out Guid guid))
            {
                return Ok(new List<OrdenDto>());
            }

            return Ok(await _service.GetAllAsync(guid));
        }

        // GET by id
        /// <summary>Obtener el detalle de una orden</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrdenDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(string id)
        {
            var orden = await _service.GetByIdAsync(ParseId(id));
            return Ok(orden);
        }

        // POST 
        /// <summary>Crear una orden nueva</summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(OrdenDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Crear([FromBody] CrearOrdenDto req)
        {
            ValidarBody(req);

            var orden = await _service.CrearAsync(req);
            return Created($"/api/orders/{orden.Id}", orden);
        }

        // PUT 
        /// <summary>Actualizar el estado de una orden</summary>
        [HttpPut("{id}/status")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(EstadoActualizadoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ActualizarEstado(string id, [FromBody] ActualizarEstadoDto req)
        {
            ValidarBody(req);

            var resultado = await _service.ActualizarEstadoAsync(ParseId(id), req);
            return Ok(resultado);
        }

        // Si el id no tiene formato de Guid esa orden no existe: ORD-001
        private static Guid ParseId(string id)
        {
            if (Guid.TryParse(id, out Guid guid))
            {
                return guid;
            }

            throw new NotFoundException(ErrorCodes.ORD_001, "Orden no encontrada.");
        }

        // Si el body vino vacío o con un JSON mal escrito, req llega en null: error ORD-002
        private static void ValidarBody(object? req)
        {
            if (req == null)
            {
                throw new ValidationException(ErrorCodes.ORD_002,
                    "El cuerpo del pedido falta o no es un JSON válido.");
            }
        }
    }
}