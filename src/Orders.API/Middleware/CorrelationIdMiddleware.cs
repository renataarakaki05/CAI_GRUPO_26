using Serilog.Context;

namespace Orders.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si viene en el header lo usamos si no, creamos uno nuevo
            string correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            // Lo guardo en TraceIdentifier de ahí lo leen los handlers de errores
            context.TraceIdentifier = correlationId;

            // Lo devuelvo también en el header de la respuesta
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Correlation-Id"] = correlationId;
                return Task.CompletedTask;
            });

            string endpoint = $"{context.Request.Method} {context.Request.Path}";

            // Todo lo que se loguee durante este request va a llevar el CorrelationId y el Endpoint
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("Endpoint", endpoint))
            {
                // El fin del request lo loguea UseSerilogRequestLogging
                _logger.LogInformation("Inicio request {Endpoint}", endpoint);
                await _next(context);
            }
        }
    }
}