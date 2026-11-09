using Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers
{
    // Agarra las NotFoundException y responde 404 CRT-001 CRT-002
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<NotFoundExceptionHandler> _logger;

        public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            // Si no es una NotFoundException, devolvemos false y la agarra el próximo handler
            var ex = exception as NotFoundException;
            if (ex == null)
            {
                return false;
            }

            // Es un error esperado del negocio, así que va como Warning
            _logger.LogWarning("{ErrorCode} {ErrorMessage}", ex.ErrorCode, ex.Message);

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
