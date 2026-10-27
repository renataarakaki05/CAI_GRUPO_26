using Serilog.Context;

namespace Products.API.Middleware
{
    // Genera (o reutiliza) el X-Correlation-Id de cada request, lo agrega a todos los logs
    // del request y loggea el inicio.
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";
        internal const string ItemKey = "CorrelationId";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si otro microservicio ya mandó un Correlation ID, se respeta
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value.ToString()
                : Guid.NewGuid().ToString();

            context.Items[ItemKey] = correlationId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            var endpoint = $"{context.Request.Method} {context.Request.Path}";

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("Endpoint", endpoint))
            {
                // El fin del request con su duración lo loggea UseSerilogRequestLogging
                _logger.LogInformation("Inicio request {Endpoint}", endpoint);
                await _next(context);
            }
        }
    }

    public static class CorrelationIdExtensions
    {
        public static string? GetCorrelationId(this HttpContext context) =>
            context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var id) ? id as string : null;
    }

    // Propaga el Correlation ID en las llamadas HTTP salientes (IHttpClientFactory)
    public class CorrelationIdDelegatingHandler(IHttpContextAccessor accessor) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var correlationId = accessor.HttpContext?.GetCorrelationId();
            if (correlationId is not null && !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
                request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

            return base.SendAsync(request, ct);
        }
    }
}
