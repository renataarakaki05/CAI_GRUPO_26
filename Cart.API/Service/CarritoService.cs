using Cart.API.Clients;
using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;

namespace Cart.API.Services
{
    // aca esta la logica del carrito: valida, consulta a products y guarda en la base
    // si algo sale mal tira una excepcion y el handler arma la respuesta de error
    public class CarritoService
    {
        private readonly CarritoRepository _repo;
        private readonly ProductsClient _products;
        private readonly ILogger<CarritoService> _logger;

        public CarritoService(CarritoRepository repo, ProductsClient products, ILogger<CarritoService> logger)
        {
            _repo = repo;
            _products = products;
            _logger = logger;
        }

        // mostramos el carrito
        public async Task<CarritoDto> GetAsync(Guid usuarioId)
        {
            var carrito = await GetCarritoExistenteAsync(usuarioId);
            return CarritoDto.FromEntity(carrito);
        }

        // agregaos el carrito
        public async Task<CarritoDto> AgregarItemAsync(Guid usuarioId, AgregarItemDto request, CancellationToken ct)
        {
            // si el body vino vacio o mal escrito llega null
            if (request == null)
            {
                throw new ValidationException(ErrorCodes.CRT_004,
                    "Cantidad inválida: el cuerpo de la request falta o no es un JSON válido.");
            }

            ValidarCantidad(request.Cantidad);

            // le preguntamos a products si existe el producto
            var producto = await GetProductoExistenteAsync(request.ProductoId, ct);

            // si el usuario no tiene carrito todavia se lo creamos
            var carrito = await _repo.GetByUsuarioAsync(usuarioId);
            if (carrito == null)
            {
                carrito = new Carrito();
                carrito.UsuarioId = usuarioId;
                carrito.FechaActualizacion = DateTime.UtcNow;
                await _repo.CreateAsync(carrito);
                _logger.LogInformation("Carrito creado para el usuario {UsuarioId}", usuarioId);
            }

            // si el producto ya esta en el carrito sumamos la cantidad
            var itemExistente = BuscarItem(carrito, request.ProductoId);

            var cantidadTotal = request.Cantidad;
            if (itemExistente != null)
            {
                cantidadTotal = itemExistente.Cantidad + request.Cantidad;
            }

            ValidarStock(producto, cantidadTotal);

            if (itemExistente == null)
            {
                var nuevo = new CarritoItem();
                nuevo.ProductoId = request.ProductoId;
                nuevo.Cantidad = cantidadTotal;
                await _repo.AddItemAsync(usuarioId, nuevo);
            }
            else
            {
                itemExistente.Cantidad = cantidadTotal;
                await _repo.UpdateItemAsync(usuarioId, itemExistente);
            }

            await _repo.UpdateFechaAsync(usuarioId, DateTime.UtcNow);
            _logger.LogInformation("Producto {ProductoId} agregado al carrito de {UsuarioId}", request.ProductoId, usuarioId);

            // devolvemos el carrito como quedo
            return await GetAsync(usuarioId);
        }

        // cambiamos las cantidades
        public async Task<CarritoDto> ActualizarCantidadAsync(Guid usuarioId, Guid productoId, ActualizarCantidadDto request, CancellationToken ct)
        {
            if (request == null)
            {
                throw new ValidationException(ErrorCodes.CRT_004,
                    "Cantidad inválida: el cuerpo de la request falta o no es un JSON válido.");
            }

            ValidarCantidad(request.Cantidad);

            var carrito = await GetCarritoExistenteAsync(usuarioId);

            // el producto tiene q estar en el carrito para poder cambiarle la cantidad
            var item = BuscarItem(carrito, productoId);
            if (item == null)
            {
                throw new NotFoundException(ErrorCodes.CRT_002, "El producto no está en el carrito.");
            }

            // volvemos a mirar el stock en products por si cambio
            var producto = await GetProductoExistenteAsync(productoId, ct);
            ValidarStock(producto, request.Cantidad);

            item.Cantidad = request.Cantidad;
            await _repo.UpdateItemAsync(usuarioId, item);
            await _repo.UpdateFechaAsync(usuarioId, DateTime.UtcNow);
            _logger.LogInformation("Cantidad del producto {ProductoId} actualizada a {Cantidad} en el carrito de {UsuarioId}",
                productoId, request.Cantidad, usuarioId);

            return await GetAsync(usuarioId);
        }

        // sacamos el producto
        public async Task QuitarItemAsync(Guid usuarioId, Guid productoId)
        {
            var carrito = await GetCarritoExistenteAsync(usuarioId);

            var item = BuscarItem(carrito, productoId);
            if (item == null)
            {
                throw new NotFoundException(ErrorCodes.CRT_002, "El producto no está en el carrito.");
            }

            await _repo.DeleteItemAsync(usuarioId, productoId);
            await _repo.UpdateFechaAsync(usuarioId, DateTime.UtcNow);
            _logger.LogInformation("Producto {ProductoId} quitado del carrito de {UsuarioId}", productoId, usuarioId);
        }

        // vaciamos el carrito
        public async Task VaciarAsync(Guid usuarioId)
        {
            // si no tiene carrito tiramos CRT-001
            await GetCarritoExistenteAsync(usuarioId);

            await _repo.DeleteAllItemsAsync(usuarioId);
            await _repo.UpdateFechaAsync(usuarioId, DateTime.UtcNow);
            _logger.LogInformation("Carrito de {UsuarioId} vaciado", usuarioId);
        }

        // metodos q nos ayudan

        // trae el carrito y si no existe tira CRT-001
        private async Task<Carrito> GetCarritoExistenteAsync(Guid usuarioId)
        {
            var carrito = await _repo.GetByUsuarioAsync(usuarioId);
            if (carrito == null)
            {
                throw new NotFoundException(ErrorCodes.CRT_001, "Carrito no encontrado.");
            }

            return carrito;
        }

        // busca el producto en products y si no existe tira CRT-002
        private async Task<ProductoInfo> GetProductoExistenteAsync(Guid productoId, CancellationToken ct)
        {
            var producto = await _products.GetProductoAsync(productoId, ct);
            if (producto == null)
            {
                throw new NotFoundException(ErrorCodes.CRT_002, "Producto no encontrado.");
            }

            return producto;
        }

        // recorremos los items a mano para buscar el producto (devuelve null si no esta)
        private static CarritoItem? BuscarItem(Carrito carrito, Guid productoId)
        {
            foreach (var item in carrito.Items)
            {
                if (item.ProductoId == productoId)
                {
                    return item;
                }
            }

            return null;
        }

        // la cantidad tiene q ser mayor a 0, si no CRT-004
        private static void ValidarCantidad(int cantidad)
        {
            if (cantidad <= 0)
            {
                throw new ValidationException(ErrorCodes.CRT_004, "Cantidad inválida. Debe ser mayor a 0.");
            }
        }

        // si piden mas de lo q hay en stock tiramos CRT-003
        private static void ValidarStock(ProductoInfo producto, int cantidadPedida)
        {
            if (cantidadPedida > producto.Stock)
            {
                throw new BusinessRuleException(ErrorCodes.CRT_003,
                    $"Stock insuficiente. Disponible: {producto.Stock}, solicitado: {cantidadPedida}.");
            }
        }
    }
}