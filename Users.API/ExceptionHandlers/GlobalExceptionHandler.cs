using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    // Agarra cualquier error que no atraparon los otros handlers y devuelve 500
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
            // Si es inesperado se loguea como Error
            _logger.LogError(exception, "{ErrorCode} Error inesperado: {Message}", ErrorCodes.USR_006, exception.Message);

            // Solo en appsettings.Development.json se muestra el mensaje del error.
            // Nunca se muestra el stack trace
            bool mostrarDetalle = _config.GetValue<bool>("ErrorHandling:IncludeExceptionDetails");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error inesperado.",
                instance = context.Request.Path.Value,
                errorCode = ErrorCodes.USR_006,
                errorMessage = "Error interno al procesar el usuario.",
                correlationId = context.TraceIdentifier,
                exceptionDetail = mostrarDetalle ? exception.Message : null
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
