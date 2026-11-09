using Orders.API.Clients;
using Orders.API.Data;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services
{
    public class OrdenService
    {
        private static readonly List<string> EstadosValidos = new List<string>
        {
            "Pendiente", "Confirmada", "Enviada", "Entregada", "Cancelada"
        };

        private readonly OrdenRepository _repo;
        private readonly UsersClient _usersClient;
        private readonly ProductsClient _productsClient;
        private readonly ILogger<OrdenService> _logger;

        public OrdenService(OrdenRepository repo, UsersClient usersClient,
            ProductsClient productsClient, ILogger<OrdenService> logger)
        {
            _repo = repo;
            _usersClient = usersClient;
            _productsClient = productsClient;
            _logger = logger;
        }

        // CREAR
        public async Task<OrdenDto> CrearAsync(CrearOrdenDto request)
        {
            Validar(request);

            // El usuario tiene que existir en Users.API: si no, error ORD-003
            bool existeUsuario = await _usersClient.ExisteUsuarioAsync(request.UsuarioId);
            if (!existeUsuario)
            {
                throw new NotFoundException(ErrorCodes.ORD_003, "Usuario no encontrado al crear la orden.");
            }

            var orden = new Orden();
            orden.Id = Guid.NewGuid();
            orden.UsuarioId = request.UsuarioId;
            orden.Estado = "Pendiente";
            orden.FechaCreacion = DateTime.UtcNow;

            foreach (var itemPedido in request.Items)
            {
                // Cada producto tiene que existir en Products.API si no, error ORD-004
                var producto = await _productsClient.GetProductoAsync(itemPedido.ProductoId);
                if (producto == null)
                {
                    throw new NotFoundException(ErrorCodes.ORD_004, "Producto no encontrado al crear la orden.");
                }

                // Tiene que haber stock suficiente si no, error ORD-005
                if (producto.Stock < itemPedido.Cantidad)
                {
                    throw new BusinessRuleException(ErrorCodes.ORD_005,
                        $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, solicitado: {itemPedido.Cantidad}.");
                }

                var item = new OrdenItem();
                item.ProductoId = producto.Id;
                item.Cantidad = itemPedido.Cantidad;
                item.PrecioUnitario = producto.Precio; // Se guarda el precio de hoy

                orden.Items.Add(item);
                orden.Total = orden.Total + (item.PrecioUnitario * item.Cantidad);
            }

            await _repo.CreateAsync(orden);
            _logger.LogInformation("Orden {OrdenId} creada para el usuario {UsuarioId}", orden.Id, orden.UsuarioId);

            return ConvertirADto(orden);
        }

        // LISTAR
        public async Task<List<OrdenDto>> GetAllAsync(Guid? usuarioId)
        {
            var ordenes = await _repo.GetAllAsync(usuarioId);

            var resultado = new List<OrdenDto>();
            foreach (var orden in ordenes)
            {
                resultado.Add(ConvertirADto(orden));
            }

            return resultado;
        }

        // BUSCAR POR ID
        public async Task<OrdenDto> GetByIdAsync(Guid id)
        {
            var orden = await _repo.GetByIdAsync(id);

            // Si no existe: error ORD-001 (404)
            if (orden == null)
            {
                throw new NotFoundException(ErrorCodes.ORD_001, "Orden no encontrada.");
            }

            return ConvertirADto(orden);
        }

        // CAMBIAR ESTADO
        public async Task<EstadoActualizadoDto> ActualizarEstadoAsync(Guid id, ActualizarEstadoDto request)
        {
            // El estado nuevo tiene que ser uno de los permitidos si no, error ORD-002
            if (string.IsNullOrWhiteSpace(request.Estado) || !EstadosValidos.Contains(request.Estado))
            {
                throw new ValidationException(ErrorCodes.ORD_002,
                    $"Los datos de la orden son inválidos: el estado tiene que ser {string.Join(", ", EstadosValidos)}.");
            }

            // La orden tiene que existir si no, error ORD-001
            var orden = await _repo.GetByIdAsync(id);
            if (orden == null)
            {
                throw new NotFoundException(ErrorCodes.ORD_001, "Orden no encontrada.");
            }

            // El cambio tiene que estar permitido si no, error ORD-006
            if (!EsCambioPermitido(orden.Estado, request.Estado))
            {
                throw new BusinessRuleException(ErrorCodes.ORD_006,
                    $"Una orden en estado '{orden.Estado}' no puede pasar a '{request.Estado}'.");
            }

            DateTime fechaActualizacion = DateTime.UtcNow;
            await _repo.UpdateEstadoAsync(id, request.Estado, fechaActualizacion);
            _logger.LogInformation("Orden {OrdenId} pasó de {EstadoAnterior} a {EstadoNuevo}",
                id, orden.Estado, request.Estado);

            return new EstadoActualizadoDto(id, request.Estado, fechaActualizacion);
        }

        private static bool EsCambioPermitido(string estadoActual, string estadoNuevo)
        {
            if (estadoActual == "Pendiente")
                return estadoNuevo == "Confirmada" || estadoNuevo == "Cancelada";

            if (estadoActual == "Confirmada")
                return estadoNuevo == "Enviada" || estadoNuevo == "Cancelada";

            if (estadoActual == "Enviada")
                return estadoNuevo == "Entregada";

            return false;
        }

        // Pasa la orden al DTO que devuelve la API
        private static OrdenDto ConvertirADto(Orden orden)
        {
            var items = new List<OrdenItemDto>();
            foreach (var item in orden.Items)
            {
                items.Add(new OrdenItemDto(item.ProductoId, item.Cantidad, item.PrecioUnitario));
            }

            // Para que salga con la Z al final
            return new OrdenDto(orden.Id, orden.UsuarioId, items, orden.Total, orden.Estado,
                DateTime.SpecifyKind(orden.FechaCreacion, DateTimeKind.Utc));
        }

        // Revisa los datos de la orden. Si hay errores tira ORD-002 
        private static void Validar(CrearOrdenDto request)
        {
            var errores = new List<string>();

            if (request.UsuarioId == Guid.Empty)
                errores.Add("El usuarioId es requerido");

            if (request.Items == null || request.Items.Count == 0)
            {
                errores.Add("La orden tiene que tener al menos un producto");
            }
            else
            {
                var productosVistos = new List<Guid>();

                foreach (var item in request.Items)
                {
                    if (item.ProductoId == Guid.Empty)
                        errores.Add("El productoId es requerido");
                    else if (productosVistos.Contains(item.ProductoId))
                        errores.Add($"El producto {item.ProductoId} está repetido");

                    if (item.Cantidad <= 0)
                        errores.Add("La cantidad tiene que ser mayor a 0");

                    productosVistos.Add(item.ProductoId);
                }
            }

            if (errores.Count > 0)
            {
                throw new ValidationException(ErrorCodes.ORD_002,
                    $"Los datos de la orden son inválidos: {string.Join("; ", errores)}.");
            }
        }
    }
}