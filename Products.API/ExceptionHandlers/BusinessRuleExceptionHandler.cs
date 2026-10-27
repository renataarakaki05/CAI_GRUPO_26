using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            if (exception is not BusinessRuleException ex) return false;

            logger.LogWarning("{ErrorCode} {ErrorMessage}", ex.ErrorCode, ex.Message);

            // PRD-003 es un duplicado al crear; PRD-004 es un borrado que no se permite
            var detail = ex.ErrorCode == ErrorCodes.PRD_004
                ? "No se puede eliminar el recurso."
                : "Ya existe un recurso con esos datos.";

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
