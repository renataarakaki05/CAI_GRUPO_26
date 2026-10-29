using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            string errorCode;
            string errorMessage;

            switch (exception)
            {
                // Datos que no cumplen las validaciones del DTO (los lanza NotificacionService)
                case ValidationException ex:
                    errorCode = ex.ErrorCode;
                    errorMessage = ex.Message;
                    break;

                // Body vacío o JSON mal formado (lo lanza ASP.NET Core antes de llegar al endpoint)
                case BadHttpRequestException:
                    errorCode = ErrorCodes.NTF_002;
                    errorMessage = "Los datos de la notificación son inválidos: el cuerpo de la request falta o no es un JSON válido.";
                    break;

                default:
                    return false;
            }

            logger.LogWarning("{ErrorCode} {ErrorMessage}", errorCode, errorMessage);

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
