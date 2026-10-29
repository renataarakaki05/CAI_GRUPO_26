using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    // Red de seguridad: atrapa cualquier excepción que no manejaron los handlers anteriores
    // (incluido Users.API caído o respondiendo con error).
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration config) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            // Error inesperado → Error (el stack trace queda solo en el log, nunca en la respuesta)
            logger.LogError(exception, "{ErrorCode} Error inesperado: {Message}", ErrorCodes.NTF_004, exception.Message);

            var includeDetails = config.GetValue<bool>("ErrorHandling:IncludeExceptionDetails");

            await ErrorResponseWriter.WriteAsync(context,
                status: StatusCodes.Status500InternalServerError,
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title: "Internal Server Error",
                detail: "Ocurrió un error inesperado.",
                errorCode: ErrorCodes.NTF_004,
                errorMessage: "Error interno al procesar la notificación.",
                ct: ct,
                exceptionDetail: includeDetails ? exception.Message : null);

            return true;
        }
    }
}
