using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Services;

namespace Users.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Tags("Users")]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuariosController(UsuarioService service)
        {
            _service = service;
        }

        /// <summary>Registrar nuevo usuario</summary>
        [HttpPost("register")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDto req)
        {
            ValidarBody(req);

            var usuario = await _service.RegistrarAsync(req);
            return Created($"/api/users/{usuario.Id}", usuario);
        }

        /// <summary>Autenticar usuario (email + password)</summary>
        [HttpPost("login")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginDto req)
        {
            ValidarBody(req);

            var usuario = await _service.LoginAsync(req);
            return Ok(usuario);
        }

        /// <summary>Obtener usuario por ID (lo usan Orders.API y Notifications.API)</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(string id)
        {
            // Si el id no tiene formato de Guid, ese usuario no existe: USR-007
            if (!Guid.TryParse(id, out Guid guid))
            {
                throw new NotFoundException(ErrorCodes.USR_007, "Usuario no encontrado.");
            }

            var usuario = await _service.GetByIdAsync(guid);
            return Ok(usuario);
        }

        // Si el body vino vacío o con un JSON mal escrito, req llega en null: error USR-002
        private static void ValidarBody(object? req)
        {
            if (req == null)
            {
                throw new ValidationException(ErrorCodes.USR_002,
                    "El cuerpo del pedido falta o no es un JSON válido.");
            }
        }
    }
}