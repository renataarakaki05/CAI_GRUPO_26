using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers
{
    // Agarra los BusinessRuleException. Segun el codigo devuelve 401, 403 o 409
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

            // USR-001: el email ya existe
            int status = 409;
            string type = "https://tools.ietf.org/html/rfc7231#section-6.5.9";
            string title = "Conflict";
            string detail = "Ya existe un recurso con esos datos.";

            if (ex.ErrorCode == ErrorCodes.USR_003)
            {
                // Email o contraseña incorrectos
                status = 401;
                type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                title = "Unauthorized";
                detail = "Las credenciales no son válidas.";
            }
            else if (ex.ErrorCode == ErrorCodes.USR_004 || ex.ErrorCode == ErrorCodes.USR_005)
            {
                // Usuario bloqueado
                status = 403;
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3";
                title = "Forbidden";
                detail = "El acceso está prohibido.";
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