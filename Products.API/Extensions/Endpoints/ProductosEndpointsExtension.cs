using System.ComponentModel.DataAnnotations;
using Products.API.Clients;
using Products.API.Data;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;
using Products.API.Swagger;
using ValidationException = Products.API.Exceptions.ValidationException;

namespace Products.API.Extensions.Endpoints
{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad
    public static class ProductosEndpointsExtension
    {
        public static void MapProductoEndpoints(this WebApplication app)
        {
            // GET all -- con filtros opcionales ?categoria= y ?nombre=
            app.MapGet("/api/products", async (string? categoria, string? nombre, ProductoRepository repo) =>
            {
                var productos = await repo.GetAllAsync();
                var resultado = new List<ProductoDto>();

                foreach (var producto in productos)
                {
                    if (!string.IsNullOrWhiteSpace(categoria) &&
                        !producto.Categoria.Equals(categoria.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(nombre) &&
                        !producto.Nombre.Contains(nombre.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    resultado.Add(ProductoDto.FromEntity(producto));
                }

                return Results.Ok(resultado);
            })
            .WithTags("Products")
            .WithSummary("Listar productos (filtro ?categoria= y ?nombre=)")
            .Produces<IEnumerable<ProductoDto>>(StatusCodes.Status200OK)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));

            // GET by id -- la parte del URL
            app.MapGet("/api/products/{id}", async (string id, ProductoRepository repo) =>
            {
                var producto = await BuscarProducto(ParseId(id), repo);
                return Results.Ok(ProductoDto.FromEntity(producto));
            })
            .WithTags("Products")
            .WithSummary("Obtener producto por ID")
            .Produces<ProductoDto>(StatusCodes.Status200OK)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.PRD_001))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));

            // POST -- PARA CREAR
            app.MapPost("/api/products", async (CrearProductoDto req, ProductoRepository repo, ILogger<Program> logger) =>
            {
                Validar(req);

                var nombre = req.Nombre.Trim();
                var categoria = req.Categoria.Trim();

                var existentes = await repo.GetAllAsync();
                foreach (var existente in existentes)
                {
                    if (existente.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) &&
                        existente.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BusinessRuleException(ErrorCodes.PRD_003,
                            $"Ya existe un producto con ese nombre en la categoría '{categoria}'.");
                    }
                }

                var producto = new Producto();
                producto.Id = Guid.NewGuid();
                producto.Nombre = nombre;
                producto.Descripcion = req.Descripcion;
                producto.Precio = req.Precio!.Value;
                producto.Stock = req.Stock!.Value;
                producto.Categoria = categoria;
                producto.FechaCreacion = DateTime.UtcNow;

                await repo.CreateAsync(producto);
                logger.LogInformation("Producto {ProductoId} creado", producto.Id);

                return Results.Created($"/api/products/{producto.Id}", ProductoDto.FromEntity(producto));
            })
            .WithTags("Products")
            .WithSummary("Crear nuevo producto")
            .Produces<ProductoDto>(StatusCodes.Status201Created)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status400BadRequest, ErrorCodes.PRD_002))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status409Conflict, ErrorCodes.PRD_003))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));

            // PUT -- PARA MODIFICAR POR ESO NECESITO EL ID
            app.MapPut("/api/products/{id}", async (string id, CrearProductoDto req, ProductoRepository repo, ILogger<Program> logger) =>
            {
                var productoId = ParseId(id);
                Validar(req);
                var producto = await BuscarProducto(productoId, repo);

                producto.Nombre = req.Nombre.Trim();
                producto.Descripcion = req.Descripcion;
                producto.Precio = req.Precio!.Value;
                producto.Stock = req.Stock!.Value;
                producto.Categoria = req.Categoria.Trim();

                await repo.UpdateAsync(producto);
                logger.LogInformation("Producto {ProductoId} actualizado", productoId);

                return Results.Ok(ProductoDto.FromEntity(producto));
            })
            .WithTags("Products")
            .WithSummary("Actualizar producto existente")
            .Produces<ProductoDto>(StatusCodes.Status200OK)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status400BadRequest, ErrorCodes.PRD_002))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.PRD_001))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));

            // DELETE -- no se puede si el producto está en órdenes Pendiente o Confirmada
            app.MapDelete("/api/products/{id}", async (string id, ProductoRepository repo, OrdersClient orders, ILogger<Program> logger, CancellationToken ct) =>
            {
                var productoId = ParseId(id);
                await BuscarProducto(productoId, repo);

                if (await orders.TieneOrdenesActivasAsync(productoId, ct))
                {
                    throw new BusinessRuleException(ErrorCodes.PRD_004,
                        "El producto tiene órdenes activas y no puede eliminarse.");
                }

                await repo.DeleteAsync(productoId);
                logger.LogInformation("Producto {ProductoId} eliminado", productoId);

                return Results.NoContent();
            })
            .WithTags("Products")
            .WithSummary("Eliminar producto")
            .Produces(StatusCodes.Status204NoContent)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.PRD_001))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status409Conflict, ErrorCodes.PRD_004))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));
        }

        // Un ID que no es un Guid válido (ej: /api/products/99) tampoco existe → PRD-001
        private static Guid ParseId(string id)
        {
            if (Guid.TryParse(id, out var guid))
            {
                return guid;
            }

            throw new NotFoundException(ErrorCodes.PRD_001, "Producto no encontrado.");
        }

        private static async Task<Producto> BuscarProducto(Guid id, ProductoRepository repo)
        {
            var producto = await repo.GetByIdAsync(id);
            if (producto == null)
            {
                throw new NotFoundException(ErrorCodes.PRD_001, "Producto no encontrado.");
            }

            return producto;
        }

        // Valida las Data Annotations del DTO; todos los problemas van separados por punto y coma (PRD-002)
        private static void Validar(CrearProductoDto req)
        {
            var resultados = new List<ValidationResult>();
            if (Validator.TryValidateObject(req, new ValidationContext(req), resultados, validateAllProperties: true))
            {
                return;
            }

            var errores = new List<string?>();
            foreach (var resultado in resultados)
            {
                errores.Add(resultado.ErrorMessage);
            }

            throw new ValidationException(ErrorCodes.PRD_002,
                $"Los datos del producto son inválidos: {string.Join("; ", errores)}.");
        }
    }
}
