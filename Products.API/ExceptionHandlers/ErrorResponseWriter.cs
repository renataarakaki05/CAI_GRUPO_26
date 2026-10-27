using Products.API.Middleware;

namespace Products.API.ExceptionHandlers
{
    // Arma la respuesta de error con el contrato de la sección 3.1 de la consigna.
    // La comparten todos los IExceptionHandler.
    public static class ErrorResponseWriter
    {
        public static async Task WriteAsync(
            HttpContext context,
            int status,
            string type,
            string title,
            string detail,
            string errorCode,
            string errorMessage,
            CancellationToken ct,
            string? exceptionDetail = null)
        {
            var body = new Dictionary<string, object?>
            {
                ["type"] = type,
                ["title"] = title,
                ["status"] = status,
                ["detail"] = detail,
                ["instance"] = context.Request.Path.Value,
                ["errorCode"] = errorCode,
                ["errorMessage"] = errorMessage,
                ["correlationId"] = context.GetCorrelationId()
            };

            // Solo en Development (ver appsettings.Development.json); nunca se manda el stack trace
            if (exceptionDetail is not null)
                body["exceptionDetail"] = exceptionDetail;

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(body, options: null, contentType: "application/problem+json", cancellationToken: ct);
        }
    }
}
