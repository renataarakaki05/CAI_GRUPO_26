using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class NotFoundExceptionHandler : IExceptionHandler // Agarra los NotFoundException y devuelve 404
    {
        private readonly ILogger<NotFoundExceptionHandler> _logger;

        public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not NotFoundException ex) return false; 

            _logger.LogWarning("{ErrorCode} {ErrorMessage}", ex.ErrorCode, ex.Message);  // Error de negocio se loggea como warning

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