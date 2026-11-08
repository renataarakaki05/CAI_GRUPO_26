using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Notifications.API.Swagger
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
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

    // Agrega a Swagger el ejemplo de response de error de cada [ErrorExample]
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
            ["NTF-001"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found",
                "El recurso solicitado no fue encontrado.",
                "El usuario destinatario no fue encontrado."),
            ["NTF-002"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request",
                "Los datos enviados no son válidos.",
                "Los datos de la notificación son inválidos: El mensaje es requerido; El tipo debe ser Email, Push o SMS."),
            ["NTF-003"] = new("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found",
                "El recurso solicitado no fue encontrado.",
                "No se encontraron notificaciones para el usuario."),
            ["NTF-004"] = new("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error",
                "Ocurrió un error inesperado.",
                "Error interno al procesar la notificación.")
        };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // El instance sale de la ruta del endpoint, porque un mismo código puede usarse en varios endpoints
            var instance = "/" + context.ApiDescription.RelativePath;
            instance = instance.Replace("{userId}", "a1b2c3d4-0000-0000-0000-111122223333");

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
