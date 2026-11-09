namespace Cart.API.ExceptionHandlers
{
    // Arma y escribe la respuesta de error. La usan todos los handlers,
    // asi no repetimos el mismo código en cada uno.
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
            // Llenamos el molde con los datos del error
            var body = new ErrorResponse();
            body.Type = type;
            body.Title = title;
            body.Status = status;
            body.Detail = detail;
            body.Instance = context.Request.Path.Value;
            body.ErrorCode = errorCode;
            body.ErrorMessage = errorMessage;
            body.CorrelationId = context.TraceIdentifier;
            body.ExceptionDetail = exceptionDetail;

            // Ponemos el código HTTP 404 422 y mandamos el JSON
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(body, options: null, contentType: "application/problem+json", cancellationToken: ct);
        }
    }
}
