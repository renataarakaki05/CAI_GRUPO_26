using System.Text.Json;
using Cart.API.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cart.API.Swagger
{
    // etiqueta q se le pone a cada endpoint para decir q error puede devolver
   
    // hereda de ProducesResponseType asi swagger ya sabe q ese codigo devuelve un ErrorResponse
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ErrorExampleAttribute : ProducesResponseTypeAttribute
    {
        public string ErrorCode { get; }

        public ErrorExampleAttribute(int statusCode, string errorCode)
            : base(typeof(ErrorResponse), statusCode, "application/problem+json")
        {
            ErrorCode = errorCode;
        }
    }

    // swagger pasa por aca con cada endpoint y le agregamos el ejemplo de cada error
    public class ErrorExamplesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Responses == null)
            {
                return;
            }

            // el instance del ejemplo es la ruta del endpoint con ids de ejemplo
            var instance = "/" + context.ApiDescription.RelativePath;
            instance = instance.Replace("{userId}", "a1b2c3d4-0000-0000-0000-111122223333");
            instance = instance.Replace("{productId}", "3fa85f64-5717-4562-b3fc-2c963f66afa6");

            // recorremos las etiquetas ErrorExample q tiene el endpoint
            foreach (var metadata in context.ApiDescription.ActionDescriptor.EndpointMetadata)
            {
                var attr = metadata as ErrorExampleAttribute;
                if (attr == null)
                {
                    continue;
                }

                // la respuesta ya la armo swagger gracias a la etiqueta
                var codigoHttp = attr.StatusCode.ToString();
                if (!operation.Responses.ContainsKey(codigoHttp))
                {
                    continue;
                }

                var response = operation.Responses[codigoHttp] as OpenApiResponse;
                if (response == null)
                {
                    continue;
                }

                var ejemplo = ArmarEjemplo(attr.StatusCode, attr.ErrorCode, instance);

                // si ya habia otro error con el mismo codigo http los mostramos juntos
                var texto = attr.ErrorCode + ": " + ejemplo.ErrorMessage;
                if (response.Description != null && response.Description.StartsWith("CRT-"))
                {
                    response.Description = response.Description + " | " + texto;
                    continue;
                }

                response.Description = texto;

                // pasamos el ejemplo a json y se lo ponemos a la respuesta
                var json = JsonSerializer.SerializeToNode(ejemplo, JsonSerializerOptions.Web);
                if (response.Content != null)
                {
                    foreach (var mediaType in response.Content.Values)
                    {
                        mediaType.Example = json;
                    }
                }
            }
        }

        // arma el json de ejemplo de cada error, usando el mismo molde q las respuestas de verdad
        private static ErrorResponse ArmarEjemplo(int status, string errorCode, string instance)
        {
            var ejemplo = new ErrorResponse();
            ejemplo.Status = status;
            ejemplo.Instance = instance;
            ejemplo.ErrorCode = errorCode;
            ejemplo.CorrelationId = "0f8fad5b-d9cb-469f-a165-70867728950e";

            if (errorCode == "CRT-001")
            {
                ejemplo.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                ejemplo.Title = "Not Found";
                ejemplo.Detail = "El recurso solicitado no fue encontrado.";
                ejemplo.ErrorMessage = "Carrito no encontrado.";
            }
            else if (errorCode == "CRT-002")
            {
                ejemplo.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                ejemplo.Title = "Not Found";
                ejemplo.Detail = "El recurso solicitado no fue encontrado.";
                ejemplo.ErrorMessage = "Producto no encontrado.";
            }
            else if (errorCode == "CRT-003")
            {
                ejemplo.Type = "https://tools.ietf.org/html/rfc4918#section-11.2";
                ejemplo.Title = "Unprocessable Entity";
                ejemplo.Detail = "No se puede procesar la solicitud.";
                ejemplo.ErrorMessage = "Stock insuficiente. Disponible: 1, solicitado: 5.";
            }
            else if (errorCode == "CRT-004")
            {
                ejemplo.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                ejemplo.Title = "Bad Request";
                ejemplo.Detail = "Los datos enviados no son válidos.";
                ejemplo.ErrorMessage = "Cantidad inválida. Debe ser mayor a 0.";
            }
            else
            {
                ejemplo.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                ejemplo.Title = "Internal Server Error";
                ejemplo.Detail = "Ocurrió un error inesperado.";
                ejemplo.ErrorMessage = "Error interno al procesar el carrito.";
            }

            return ejemplo;
        }
    }
}
