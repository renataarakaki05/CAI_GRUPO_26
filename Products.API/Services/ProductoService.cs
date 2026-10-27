using Products.API.Clients;
using Products.API.Data;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services
{
    // Lógica de negocio de productos. Los errores se lanzan como excepciones
    // de dominio y los IExceptionHandler arman la respuesta.
    public class ProductoService
    {
        private readonly ProductoRepository _repo;
        private readonly OrdersClient _orders;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(ProductoRepository repo, OrdersClient orders, ILogger<ProductoService> logger)
        {
            _repo = repo;
            _orders = orders;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductoDto>> GetAllAsync(string? categoria, string? nombre)
        {
            var products = await _repo.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(categoria))
                products = products.Where(p => p.Categoria.Equals(categoria.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(nombre))
                products = products.Where(p => p.Nombre.Contains(nombre.Trim(), StringComparison.OrdinalIgnoreCase));

            return products.Select(ProductoDto.FromEntity);
        }

        public async Task<ProductoDto> GetByIdAsync(Guid id)
        {
            var Producto = await GetExistingAsync(id);
            return ProductoDto.FromEntity(Producto);
        }

        public async Task<ProductoDto> CreateAsync(CrearProductoDto request)
        {
            var nombre = request.Nombre.Trim();
            var categoria = request.Categoria.Trim();

            var existentes = await _repo.GetAllAsync();
            if (existentes.Any(p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) &&
                                    p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)))
            {
                throw new BusinessRuleException(ErrorCodes.PRD_003,
                    $"Ya existe un producto con ese nombre en la categoría '{categoria}'.");
            }

            var Producto = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = request.Descripcion,
                Precio = request.Precio!.Value,
                Stock = request.Stock!.Value,
                Categoria = categoria,
                FechaCreacion = DateTime.UtcNow
            };

            await _repo.CreateAsync(Producto);
            _logger.LogInformation("Producto {ProductoId} creado", Producto.Id);

            return ProductoDto.FromEntity(Producto);
        }

        public async Task<ProductoDto> UpdateAsync(Guid id, CrearProductoDto request)
        {
            var Producto = await GetExistingAsync(id);

            Producto.Nombre = request.Nombre.Trim();
            Producto.Descripcion = request.Descripcion;
            Producto.Precio = request.Precio!.Value;
            Producto.Stock = request.Stock!.Value;
            Producto.Categoria = request.Categoria.Trim();

            await _repo.UpdateAsync(Producto);
            _logger.LogInformation("Producto {ProductoId} actualizado", id);

            return ProductoDto.FromEntity(Producto);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            await GetExistingAsync(id);

            if (await _orders.TieneOrdenesActivasAsync(id, ct))
                throw new BusinessRuleException(ErrorCodes.PRD_004,
                    "El producto tiene órdenes activas y no puede eliminarse.");

            await _repo.DeleteAsync(id);
            _logger.LogInformation("Producto {ProductoId} eliminado", id);
        }

        private async Task<Producto> GetExistingAsync(Guid id) =>
            await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException(ErrorCodes.PRD_001, "Producto no encontrado.");
    }
}
