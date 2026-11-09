using Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers
{
    // agarra cualquier error que no agarraron los otros handlers
    //se cayó la base o Products.API no responde y responde 500 (CRT-005)
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
            // Error que no esperábamos, va como Error
            _logger.LogError(exception, "{ErrorCode} Error inesperado: {Message}", ErrorCodes.CRT_005, exception.Message);

            // En Development mostramos el mensaje técnico para entender qué paso
            string? exceptionDetail = null;
            if (_config.GetValue<bool>("ErrorHandling:IncludeExceptionDetails"))
            {
                exceptionDetail = exception.Message;
            }

            await ErrorResponseWriter.WriteAsync(context,
                status: StatusCodes.Status500InternalServerError,
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title: "Internal Server Error",
                detail: "Ocurrió un error inesperado.",
                errorCode: ErrorCodes.CRT_005,
                errorMessage: "Error interno al procesar el carrito.",
                ct: ct,
                exceptionDetail: exceptionDetail);

            return true;
        }
    }
}
