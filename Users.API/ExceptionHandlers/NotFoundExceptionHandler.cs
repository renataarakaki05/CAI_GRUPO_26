using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    // Agarra los NotFoundException y devuelve 404
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<NotFoundExceptionHandler> _logger;

        public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            // Si no es un NotFoundException, no es para este handler
            if (exception is not NotFoundException ex) return false;

            // Warning cuando es error de negocio
            _logger.LogWarning("{ErrorCode} {ErrorMessage}", ex.ErrorCode, ex.Message);

            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = context.TraceIdentifier
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
