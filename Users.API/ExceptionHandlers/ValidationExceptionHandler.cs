using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    // Agarra los datos inválidos y devuelve 400
    public class ValidationExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ValidationExceptionHandler> _logger;

        public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            string errorCode;
            string errorMessage;

            if (exception is ValidationException ex)
            {
                // Datos que no pasaron las validaciones
                errorCode = ex.ErrorCode;
                errorMessage = ex.Message;
            }
            else if (exception is BadHttpRequestException)
            {
                // El body vino vacío o con JSON mal escrito
                errorCode = ErrorCodes.USR_002;
                errorMessage = "El cuerpo del pedido falta o no es un JSON válido.";
            }
            else
            {
                return false;
            }

            _logger.LogWarning("{ErrorCode} {ErrorMessage}", errorCode, errorMessage);

            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Los datos enviados no son válidos.",
                instance = context.Request.Path.Value,
                errorCode = errorCode,
                errorMessage = errorMessage,
                correlationId = context.TraceIdentifier
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}