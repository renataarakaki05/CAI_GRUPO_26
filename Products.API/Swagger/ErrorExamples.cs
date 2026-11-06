using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Products.API.Swagger
{
    /// <summary>
    /// Marca un endpoint con un posible error del catálogo para que Swagger muestre su ejemplo.
    /// </summary>
    public class ErrorExampleAttribute : Attribute
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        public ErrorExampleAttribute(int statusCode, string errorCode)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    // Agrega a Swagger el ejemplo de response de error de cada [ErrorExample],
    // siguiendo los contratos de la sección 4.1 de la consigna.
    public class ErrorExamplesOperationFilter : IOperationFilter
    {
        private class Example
        {
            public string Type { get; }
            public string Title { get; }
            public string Detail { get; }
            public string ErrorMessage { get; }

            public Example(string type, string title, string detail, string errorMessage)
            {
                Type = type;
                Title = title;
                Detail = detail;
                ErrorMessage = errorMessage;
            }
        }

        private static readonly Dictionary<string, Example> Catalogo = new Dictionary<string, Example>
        {
            ["PRD-001"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found",
                "El recurso solicitado no fue encontrado.",
                "Producto no encontrado."),
            ["PRD-002"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request",
                "Los datos enviados no son válidos.",
                "Los datos del producto son inválidos: El nombre es requerido; El precio debe ser mayor a 0."),
            ["PRD-003"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict",
                "Ya existe un recurso con esos datos.",
                "Ya existe un producto con ese nombre en la categoría 'Electrónica'."),
            ["PRD-004"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict",
                "No se puede eliminar el recurso.",
                "El producto tiene órdenes activas y no puede eliminarse."),
            ["PRD-005"] = new("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error",
                "Ocurrió un error inesperado.",
                "Error interno al procesar el producto.")
        };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // El instance sale de la ruta del endpoint, porque un mismo código puede usarse en varios endpoints
            var instance = "/" + context.ApiDescription.RelativePath;
            instance = instance.Replace("{id}", "3fa85f64-5717-4562-b3fc-2c963f66afa6");

            // En Minimal API el atributo se agrega con .WithMetadata(...) en cada endpoint
            foreach (var metadata in context.ApiDescription.ActionDescriptor.EndpointMetadata)
            {
                var attr = metadata as ErrorExampleAttribute;
                if (attr == null)
                {
                    continue;
                }

                var ex = Catalogo[attr.ErrorCode];

                var example = new JsonObject
                {
                    ["type"] = ex.Type,
                    ["title"] = ex.Title,
                    ["status"] = attr.StatusCode,
                    ["detail"] = ex.Detail,
                    ["instance"] = instance,
                    ["errorCode"] = attr.ErrorCode,
                    ["errorMessage"] = ex.ErrorMessage,
                    ["correlationId"] = "0f8fad5b-d9cb-469f-a165-70867728950e"
                };

                var response = new OpenApiResponse();
                response.Description = $"{attr.ErrorCode}: {ex.ErrorMessage}";
                response.Content = new Dictionary<string, OpenApiMediaType>();
                response.Content["application/problem+json"] = new OpenApiMediaType { Example = example };

                if (operation.Responses == null)
                {
                    operation.Responses = new OpenApiResponses();
                }

                operation.Responses[attr.StatusCode.ToString()] = response;
            }
        }
    }
}
