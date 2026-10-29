using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Notifications.API.Swagger
{
    /// <summary>
    /// Marca un endpoint con un posible error del catálogo para que Swagger muestre su ejemplo.
    /// </summary>
    public class ErrorExampleAttribute(int statusCode, string errorCode) : Attribute
    {
        public int StatusCode { get; } = statusCode;
        public string ErrorCode { get; } = errorCode;
    }

    // Agrega a Swagger el ejemplo de response de error de cada [ErrorExample],
    // siguiendo los contratos de la sección 4.5 de la consigna.
    public class ErrorExamplesOperationFilter : IOperationFilter
    {
        private sealed record Example(string Type, string Title, string Detail, string Instance, string ErrorMessage);

        private static readonly Dictionary<string, Example> Catalogo = new()
        {
            ["NTF-001"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found",
                "El recurso solicitado no fue encontrado.", "/api/notifications/send",
                "El usuario destinatario no fue encontrado."),
            ["NTF-002"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request",
                "Los datos enviados no son válidos.", "/api/notifications/send",
                "Los datos de la notificación son inválidos: El mensaje es requerido; El tipo debe ser Email, Push o SMS."),
            ["NTF-003"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found",
                "El recurso solicitado no fue encontrado.", "/api/notifications/a1b2c3d4-0000-0000-0000-111122223333",
                "No se encontraron notificaciones para el usuario."),
            ["NTF-004"] = new("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error",
                "Ocurrió un error inesperado.", "/api/notifications/send",
                "Error interno al procesar la notificación.")
        };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // En Minimal API el atributo se agrega con .WithMetadata(...) en cada endpoint
            var attributes = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<ErrorExampleAttribute>();

            foreach (var attr in attributes)
            {
                if (!Catalogo.TryGetValue(attr.ErrorCode, out var ex)) continue;

                var example = new JsonObject
                {
                    ["type"] = ex.Type,
                    ["title"] = ex.Title,
                    ["status"] = attr.StatusCode,
                    ["detail"] = ex.Detail,
                    ["instance"] = ex.Instance,
                    ["errorCode"] = attr.ErrorCode,
                    ["errorMessage"] = ex.ErrorMessage,
                    ["correlationId"] = "0f8fad5b-d9cb-469f-a165-70867728950e"
                };

                operation.Responses ??= new OpenApiResponses();
                var key = attr.StatusCode.ToString();

                if (!operation.Responses.TryGetValue(key, out var response) || response is not OpenApiResponse openApiResponse)
                {
                    openApiResponse = new OpenApiResponse { Description = $"{attr.ErrorCode}: {ex.ErrorMessage}" };
                    operation.Responses[key] = openApiResponse;
                }

                openApiResponse.Content ??= new Dictionary<string, OpenApiMediaType>();
                openApiResponse.Content.Clear();
                openApiResponse.Content["application/problem+json"] = new OpenApiMediaType { Example = example };
            }
        }
    }
}
