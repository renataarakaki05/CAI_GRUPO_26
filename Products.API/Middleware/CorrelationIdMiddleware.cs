using Serilog.Context;

namespace Products.API.Middleware
{

    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value.ToString()
                : Guid.NewGuid().ToString();

            context.TraceIdentifier = correlationId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            var endpoint = $"{context.Request.Method} {context.Request.Path}";

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("Endpoint", endpoint))
            {
                
                _logger.LogInformation("Inicio request {Endpoint}", endpoint);
                await _next(context);
            }
        }
    }

   
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;

        public CorrelationIdDelegatingHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var context = _accessor.HttpContext;
            if (context != null)
            {
                if (!request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
                {
                    request.Headers.Add(CorrelationIdMiddleware.HeaderName, context.TraceIdentifier);
                }
            }

            return base.SendAsync(request, ct);
        }
    }
}
