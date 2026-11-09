using Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers
{
    // Agarra los errores de datos inválidos y responde 400 -> CRT-004
    public class ValidationExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ValidationExceptionHandler> _logger;

        public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            string errorCode;
            string errorMessage;

            var ex = exception as ValidationException;
            if (ex != null)
            {
                // La tiró nuestro servicio 
                errorCode = ex.ErrorCode;
                errorMessage = ex.Message;
            }
            else if (exception is BadHttpRequestException)
            {
                // La tiró ASP.NET antes de llegar al controller 
                errorCode = ErrorCodes.CRT_004;
                errorMessage = "Cantidad inválida: el cuerpo de la request falta o no es un JSON válido.";
            }
            else
            {
                
                return false;
            }

            _logger.LogWarning("{ErrorCode} {ErrorMessage}", errorCode, errorMessage);

            await ErrorResponseWriter.WriteAsync(context,
                status: StatusCodes.Status400BadRequest,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title: "Bad Request",
                detail: "Los datos enviados no son válidos.",
                errorCode: errorCode,
                errorMessage: errorMessage,
                ct: ct);

            return true;
        }
    }
}
