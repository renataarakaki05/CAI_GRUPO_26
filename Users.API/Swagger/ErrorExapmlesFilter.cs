using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Users.API.Exceptions;

namespace Users.API.Swagger
{
    public class ErrorExamplesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            string ruta = context.ApiDescription.RelativePath ?? "";

            if (ruta == "api/users/register")
            {
                AgregarError(operation, 400, ErrorCodes.USR_002, "/api/users/register",
                    "Los datos del usuario son inválidos: El email no tiene un formato válido; La contraseña es requerida.");
                AgregarError(operation, 409, ErrorCodes.USR_001, "/api/users/register",
                    "El email 'maria@email.com' ya está registrado.");
                AgregarError(operation, 500, ErrorCodes.USR_006, "/api/users/register",
                    "Error interno al procesar el usuario.");
            }
            else if (ruta == "api/users/login")
            {
                AgregarError(operation, 400, ErrorCodes.USR_002, "/api/users/login",
                    "Los datos del usuario son inválidos: el email y la contraseña son requeridos.");
                AgregarError(operation, 401, ErrorCodes.USR_003, "/api/users/login",
                    "Credenciales incorrectas.");
                AgregarError(operation, 403, ErrorCodes.USR_004, "/api/users/login",
                    "Usuario bloqueado por demasiados intentos fallidos.");
                AgregarError(operation, 403, ErrorCodes.USR_005, "/api/users/login",
                    "Usuario bloqueado por detección de fraude.");
                AgregarError(operation, 500, ErrorCodes.USR_006, "/api/users/login",
                    "Error interno al procesar el usuario.");
            }
            else if (ruta == "api/users/{id}")
            {
                AgregarError(operation, 404, ErrorCodes.USR_007, "/api/users/a1b2c3d4-0000-0000-0000-111122223333",
                    "Usuario no encontrado.");
                AgregarError(operation, 500, ErrorCodes.USR_006, "/api/users/a1b2c3d4-0000-0000-0000-111122223333",
                    "Error interno al procesar el usuario.");
            }
        }

        // Agrega un error a la documentación de un endpoint
        private static void AgregarError(OpenApiOperation operation, int status, string errorCode,
            string instance, string errorMessage)
        {
            // Mismos datos que arman los handlers de la carpeta exceptionhandlers
            string type = "";
            string title = "";
            string detail = "";

            if (status == 400)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                title = "Bad Request";
                detail = "Los datos enviados no son válidos.";
            }
            else if (status == 401)
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                title = "Unauthorized";
                detail = "Las credenciales no son válidas.";
            }
            else if (status == 403)
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3";
                title = "Forbidden";
                detail = "El acceso está prohibido.";
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
                detail = "Ya existe un recurso con esos datos.";
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
  
            operation.Responses ??= new OpenApiResponses(); // Si el endpoint todavía no tiene ese código de respuesta lo creo
            string codigo = status.ToString();

            if (!operation.Responses.TryGetValue(codigo, out var existente) || existente is not OpenApiResponse respuesta)
            {
                respuesta = new OpenApiResponse { Description = errorCode + ": " + errorMessage };
                operation.Responses[codigo] = respuesta;
            }
            else 
            {
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