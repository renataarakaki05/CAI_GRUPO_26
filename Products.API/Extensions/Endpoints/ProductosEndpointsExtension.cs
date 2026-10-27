using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Services;
using Products.API.Swagger;

namespace Products.API.Extensions.Endpoints
{
    // ALUMNOS: clase donde se definen los endpoints para cada entidad
    public static class ProductosEndpointsExtension
    {
        public static void MapProductoEndpoints(this WebApplication app)
        {
            // GET all -- con filtros opcionales ?categoria= y ?nombre=
            app.MapGet("/api/products", async (string? categoria, string? nombre, ProductoService service) =>
            {
                var productos = await service.GetAllAsync(categoria, nombre);
                return Results.Ok(productos);
            })
            .WithTags("Products")
            .WithSummary("Listar productos (filtro ?categoria= y ?nombre=)")
            .Produces<IEnumerable<ProductoDto>>(StatusCodes.Status200OK)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));

            // GET by id -- la parte del URL
            app.MapGet("/api/products/{id}", async (string id, ProductoService service) =>
            {
                var producto = await service.GetByIdAsync(ParseId(id));
                return Results.Ok(producto);
            })
            .WithTags("Products")
            .WithSummary("Obtener producto por ID")
            .Produces<ProductoDto>(StatusCodes.Status200OK)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.PRD_001))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));

            // POST -- PARA CREAR
            app.MapPost("/api/products", async (CrearProductoDto req, ProductoService service) =>
            {
                var producto = await service.CreateAsync(req);
                return Results.Created($"/api/products/{producto.Id}", producto);
            })
            .WithTags("Products")
            .WithSummary("Crear nuevo producto")
            .Produces<ProductoDto>(StatusCodes.Status201Created)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status400BadRequest, ErrorCodes.PRD_002))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status409Conflict, ErrorCodes.PRD_003))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));

            // PUT -- PARA MODIFICAR POR ESO NECESITO EL ID
            app.MapPut("/api/products/{id}", async (string id, CrearProductoDto req, ProductoService service) =>
            {
                var producto = await service.UpdateAsync(ParseId(id), req);
                return Results.Ok(producto);
            })
            .WithTags("Products")
            .WithSummary("Actualizar producto existente")
            .Produces<ProductoDto>(StatusCodes.Status200OK)
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status400BadRequest, ErrorCodes.PRD_002))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status404NotFound, ErrorCodes.PRD_001))
            .WithMetadata(new ErrorExampleAttribute(StatusCodes.Status500InternalServerError, ErrorCodes.PRD_005));

            // DELETE -- no se puede si el producto está en órdenes Pendiente o Confirmada
            app.MapDelete("/api/products/{id}", async (string id, ProductoService service, CancellationToken ct) =>
            {
                await service.DeleteAsync(ParseId(id), ct);
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
        private static Guid ParseId(string id) =>
            Guid.TryParse(id, out var guid)
                ? guid
                : throw new NotFoundException(ErrorCodes.PRD_001, "Producto no encontrado.");
    }
}
