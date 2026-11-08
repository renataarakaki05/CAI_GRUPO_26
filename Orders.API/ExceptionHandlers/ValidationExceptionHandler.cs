using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler // Atrapa los datos inavlidos y devuelve 400
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
                errorCode = ex.ErrorCode;
                errorMessage = ex.Message;
            }
            else if (exception is BadHttpRequestException)
            {
                errorCode = ErrorCodes.ORD_002; // El body vino vacío o con un JSON mal escrito
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