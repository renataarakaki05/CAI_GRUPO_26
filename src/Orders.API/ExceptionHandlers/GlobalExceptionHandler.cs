using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    // Agarra cualq error que no atraparon los otros handlers y devuelve 500
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IConfiguration _config;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            // Error inesperado
            _logger.LogError(exception, "{ErrorCode} Error inesperado: {Message}", ErrorCodes.ORD_007, exception.Message);

            // Nunca se muestra el stack trace.
            bool mostrarDetalle = _config.GetValue<bool>("ErrorHandling:IncludeExceptionDetails");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error inesperado.",
                instance = context.Request.Path.Value,
                errorCode = ErrorCodes.ORD_007,
                errorMessage = "Error interno al procesar la orden.",
                correlationId = context.TraceIdentifier,
                exceptionDetail = mostrarDetalle ? exception.Message : null
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}