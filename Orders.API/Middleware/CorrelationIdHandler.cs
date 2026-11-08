namespace Orders.API.Middleware
{
    public class CorrelationIdHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;

        public CorrelationIdHandler(IHttpContextAccessor accessor) => _accessor = accessor;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? correlationId = _accessor.HttpContext?.TraceIdentifier; // TraceIdentifier tiene el id que guardó CorrelationIdMiddleware

            if (correlationId != null)
            {
                request.Headers.Add("X-Correlation-Id", correlationId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}