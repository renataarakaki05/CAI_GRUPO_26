using Serilog.Context;

namespace Cart.API.Middleware
{
    // le pone un X-Correlation-Id a cada request
    // si el pedido ya trae uno usamos ese
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
            // si viene en el header lo usamos, si no inventamos uno nuevo
            string correlationId;
            var valorHeader = context.Request.Headers[HeaderName].ToString();
            if (string.IsNullOrWhiteSpace(valorHeader))
            {
                correlationId = Guid.NewGuid().ToString();
            }
            else
            {
                correlationId = valorHeader;
            }

            // lo guardamos en el TraceIdentifier, de ahi lo sacan los errores y el cliente http
            context.TraceIdentifier = correlationId;

            // justo antes de mandar la respuesta le agregamos el header
            context.Response.OnStarting(AgregarHeader, context);

            var endpoint = context.Request.Method + " " + context.Request.Path;

            // todo lo q se loguee adentro de este using lleva el CorrelationId y el Endpoint
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("Endpoint", endpoint))
            {
                // el fin del request con la duracion lo loguea UseSerilogRequestLogging
                _logger.LogInformation("Inicio request {Endpoint}", endpoint);
                await _next(context);
            }
        }

        // agrega el header X-Correlation-Id a la respuesta
        private static Task AgregarHeader(object estado)
        {
            var context = (HttpContext)estado;
            context.Response.Headers[HeaderName] = context.TraceIdentifier;
            return Task.CompletedTask;
        }
    }

    // cuando cart le pega a products le manda el mismo correlation id
    // asi en los logs de los dos servicios se ve q es el mismo pedido
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
            if (context != null && !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
            {
                request.Headers.Add(CorrelationIdMiddleware.HeaderName, context.TraceIdentifier);
            }

            return base.SendAsync(request, ct);
        }
    }
}