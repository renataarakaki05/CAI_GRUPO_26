using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<BusinessRuleExceptionHandler> _logger;

        public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex) return false;

            _logger.LogWarning("{ErrorCode} {ErrorMessage}", ex.ErrorCode, ex.Message);

            // Por default ORD-006
            int status = 409;
            string type = "https://tools.ietf.org/html/rfc7231#section-6.5.9";
            string title = "Conflict";
            string detail = "No se puede modificar el estado.";

            if (ex.ErrorCode == ErrorCodes.ORD_005)
            {
                // No hay stock suficiente
                status = 422;
                type = "https://tools.ietf.org/html/rfc4918#section-11.2";
                title = "Unprocessable Entity";
                detail = "No se puede procesar la solicitud.";
            }

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                type = type,
                title = title,
                status = status,
                detail = detail,
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId = context.TraceIdentifier
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
