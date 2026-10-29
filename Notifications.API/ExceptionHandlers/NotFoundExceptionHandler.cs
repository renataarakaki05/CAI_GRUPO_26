using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            if (exception is not NotFoundException ex) return false;

            // Error de negocio → Warning
            logger.LogWarning("{ErrorCode} {ErrorMessage}", ex.ErrorCode, ex.Message);

            await ErrorResponseWriter.WriteAsync(context,
                status: StatusCodes.Status404NotFound,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title: "Not Found",
                detail: "El recurso solicitado no fue encontrado.",
                errorCode: ex.ErrorCode,
                errorMessage: ex.Message,
                ct: ct);

            return true;
        }
    }
}
