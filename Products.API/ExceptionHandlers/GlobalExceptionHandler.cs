using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers
{
    //atrapa cualquier excepción que no manejaron los handlers anteriores.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IConfiguration _config;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            
            _logger.LogError(exception, "{ErrorCode} Error inesperado: {Message}", ErrorCodes.PRD_005, exception.Message);

            var includeDetails = _config.GetValue<bool>("ErrorHandling:IncludeExceptionDetails");

            await ErrorResponseWriter.WriteAsync(context,
                status: StatusCodes.Status500InternalServerError,
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title: "Internal Server Error",
                detail: "Ocurrió un error inesperado.",
                errorCode: ErrorCodes.PRD_005,
                errorMessage: "Error interno al procesar el producto.",
                ct: ct,
                exceptionDetail: includeDetails ? exception.Message : null);

            return true;
        }
    }
}
