using Serilog.Context;

namespace Users.API.Middleware
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
            // Si viene en el header lo uso y si no creo uno nuevo
            string correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            // Lo guardo en traceidentifier pq de ahí lo leen los handlers de errores
            context.TraceIdentifier = correlationId;

            context.Response.OnStarting(() => // Lo devuelvo tambien en el header de la respuesta
            {
                context.Response.Headers["X-Correlation-Id"] = correlationId;
                return Task.CompletedTask;
            });

            string endpoint = $"{context.Request.Method} {context.Request.Path}";

            // Todo lo que se loguee durante este request va a llevar el correlationid y el endpoint
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("Endpoint", endpoint))
            {
                _logger.LogInformation("Inicio request {Endpoint}", endpoint);
                await _next(context);
            }
        }
    }
}
