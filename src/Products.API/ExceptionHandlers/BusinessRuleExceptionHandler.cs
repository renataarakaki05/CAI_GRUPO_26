using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<BusinessRuleExceptionHandler> _logger;

        public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
           
            var ex = exception as BusinessRuleException;
            if (ex == null)
            {
                return false;
            }

            _logger.LogWarning("{ErrorCode} {ErrorMessage}", ex.ErrorCode, ex.Message);

            // PRD-003 es un duplicado al crear; PRD-004 es un borrado que no se permite
            var detail = "Ya existe un recurso con esos datos.";
            if (ex.ErrorCode == ErrorCodes.PRD_004)
            {
                detail = "No se puede eliminar el recurso.";
            }

            await ErrorResponseWriter.WriteAsync(context,
                status: StatusCodes.Status409Conflict,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title: "Conflict",
                detail: detail,
                errorCode: ex.ErrorCode,
                errorMessage: ex.Message,
                ct: ct);

            return true;
        }
    }
}
