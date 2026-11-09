using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers
{
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

            if (exception is ValidationException)
            {
               
                var ex = (ValidationException)exception;
                errorCode = ex.ErrorCode;
                errorMessage = ex.Message;
            }
            else if (exception is BadHttpRequestException)
            {
                
                errorCode = ErrorCodes.PRD_002;
                errorMessage = "Los datos del producto son inválidos: el cuerpo de la request falta o no es un JSON válido.";
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
