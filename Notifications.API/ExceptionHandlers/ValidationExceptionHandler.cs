using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
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
                // Datos que no cumplen las validaciones del DTO (los lanza NotificacionService)
                var ex = (ValidationException)exception;
                errorCode = ex.ErrorCode;
                errorMessage = ex.Message;
            }
            else if (exception is BadHttpRequestException)
            {
                // Body vacío o JSON mal formado (lo lanza ASP.NET Core antes de llegar al endpoint)
                errorCode = ErrorCodes.NTF_002;
                errorMessage = "Los datos de la notificación son inválidos: el cuerpo de la request falta o no es un JSON válido.";
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
