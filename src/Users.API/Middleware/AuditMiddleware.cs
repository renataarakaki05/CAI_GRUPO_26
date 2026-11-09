namespace Users.API.Middleware
{
    // No guardo el body del request porque en Users.API ahi viaja la contraseña
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        // Solo auditar operaciones de escritura
        private static readonly HashSet<string> AuditMethods =
            new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "DELETE" };

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!AuditMethods.Contains(context.Request.Method)) // Si no es una operación de escritura, pasar directo
            {
                await _next(context);
                return;
            }

            var originalResponseBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context); // ejecutar el endpoint

            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();

            memStream.Position = 0;
            await memStream.CopyToAsync(originalResponseBody); 
            context.Response.Body = originalResponseBody;  // Copiar la respuesta de vuelta al stream original

            _logger.LogInformation( // Escribir entrada de auditoria
                "AUDIT {Method} {Path} {StatusCode} {ResponseBody}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                responseBody);
        }
    }
}
