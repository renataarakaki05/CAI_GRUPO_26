using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Orders.API.Exceptions;

namespace Orders.API.Swagger
{
    public class ErrorExamplesFilter : IOperationFilter
    {
        private const string IdOrden = "f1e2d3c4-0000-0000-0000-aabbccddeeff";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            string ruta = context.ApiDescription.RelativePath ?? "";
            string metodo = context.ApiDescription.HttpMethod ?? "";

            if (metodo == "GET" && ruta == "api/orders")
            {
                AgregarError(operation, 500, ErrorCodes.ORD_007, "/api/orders",
                    "Error interno al procesar la orden.");
            }
            else if (metodo == "POST" && ruta == "api/orders")
            {
                AgregarError(operation, 400, ErrorCodes.ORD_002, "/api/orders",
                    "Los datos de la orden son inválidos: La orden tiene que tener al menos un producto.");
                AgregarError(operation, 404, ErrorCodes.ORD_003, "/api/orders",
                    "Usuario no encontrado al crear la orden.");
                AgregarError(operation, 404, ErrorCodes.ORD_004, "/api/orders",
                    "Producto no encontrado al crear la orden.");
                AgregarError(operation, 422, ErrorCodes.ORD_005, "/api/orders",
                    "Stock insuficiente para 'Notebook Dell XPS 15'. Disponible: 2, solicitado: 5.");
                AgregarError(operation, 500, ErrorCodes.ORD_007, "/api/orders",
                    "Error interno al procesar la orden.");
            }
            else if (metodo == "GET" && ruta == "api/orders/{id}")
            {
                AgregarError(operation, 404, ErrorCodes.ORD_001, $"/api/orders/{IdOrden}",
                    "Orden no encontrada.");
                AgregarError(operation, 500, ErrorCodes.ORD_007, $"/api/orders/{IdOrden}",
                    "Error interno al procesar la orden.");
            }
            else if (metodo == "PUT" && ruta == "api/orders/{id}/status")
            {
                AgregarError(operation, 400, ErrorCodes.ORD_002, $"/api/orders/{IdOrden}/status",
                    "Los datos de la orden son inválidos: el estado tiene que ser Pendiente, Confirmada, Enviada, Entregada, Cancelada.");
                AgregarError(operation, 404, ErrorCodes.ORD_001, $"/api/orders/{IdOrden}/status",
                    "Orden no encontrada.");
                AgregarError(operation, 409, ErrorCodes.ORD_006, $"/api/orders/{IdOrden}/status",
                    "Una orden en estado 'Entregada' no puede pasar a 'Pendiente'.");
                AgregarError(operation, 500, ErrorCodes.ORD_007, $"/api/orders/{IdOrden}/status",
                    "Error interno al procesar la orden.");
            }
        }

        // Agrega un error a la documentación de un endpoint
        private static void AgregarError(OpenApiOperation operation, int status, string errorCode,
            string instance, string errorMessage)
        {
            // Mismos datos que arman los handlers de la carpeta ExceptionHandlers
            string type = "";
            string title = "";
            string detail = "";

            if (status == 400)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                title = "Bad Request";
                detail = "Los datos enviados no son válidos.";
            }
            else if (status == 404)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                title = "Not Found";
                detail = "El recurso solicitado no fue encontrado.";
            }
            else if (status == 409)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.9";
                title = "Conflict";
                detail = "No se puede modificar el estado.";
            }
            else if (status == 422)
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2";
                title = "Unprocessable Entity";
                detail = "No se puede procesar la solicitud.";
            }
            else if (status == 500)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                title = "Internal Server Error";
                detail = "Ocurrió un error inesperado.";
            }

            var ejemplo = new JsonObject
            {
                ["type"] = type,
                ["title"] = title,
                ["status"] = status,
                ["detail"] = detail,
                ["instance"] = instance,
                ["errorCode"] = errorCode,
                ["errorMessage"] = errorMessage,
                ["correlationId"] = "0f8fad5b-d9cb-469f-a165-70867728950e"
            };

            // Si el endpoint todavía no tiene ese código de respuesta, lo creamos
            operation.Responses ??= new OpenApiResponses();
            string codigo = status.ToString();

            if (!operation.Responses.TryGetValue(codigo, out var existente) || existente is not OpenApiResponse respuesta)
            {
                respuesta = new OpenApiResponse { Description = errorCode + ": " + errorMessage };
                operation.Responses[codigo] = respuesta;
            }
            else
            {
                // Un mismo status puede tener dos errores 404 del POST: ORD-003 y ORD-004
                respuesta.Description += " / " + errorCode + ": " + errorMessage;
            }

            respuesta.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!respuesta.Content.TryGetValue("application/problem+json", out var tipo))
            {
                tipo = new OpenApiMediaType();
                respuesta.Content["application/problem+json"] = tipo;
            }

            tipo.Examples ??= new Dictionary<string, IOpenApiExample>();
            tipo.Examples[errorCode] = new OpenApiExample { Summary = errorMessage, Value = ejemplo };
        }
    }
}
