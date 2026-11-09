using Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers
{
    // Agarra las BusinessRuleException y responde 422 (CRT-003, stock insuficiente)
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<BusinessRuleExceptionHandler> _logger;

        public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            // Si no es una BusinessRuleException, la pasamos al próximo handler
            var ex = exception as BusinessRuleException;
            if (ex == null)
            {
                return false;
            }

            _logger.LogWarning("{ErrorCode} {ErrorMessage}", ex.ErrorCode, ex.Message);

            await ErrorResponseWriter.WriteAsync(context,
                status: StatusCodes.Status422UnprocessableEntity,
                type: "https://tools.ietf.org/html/rfc4918#section-11.2",
                title: "Unprocessable Entity",
                detail: "No se puede procesar la solicitud.",
                errorCode: ex.ErrorCode,
                errorMessage: ex.Message,
                ct: ct);

            return true;
        }
    }
}
