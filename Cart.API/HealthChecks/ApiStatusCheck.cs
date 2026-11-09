using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks
{
    // si la api llega a responder esto es xq el proceso esta vivo
    public class ApiStatusCheck : IHealthCheck
    {
        private static readonly DateTime Inicio = DateTime.UtcNow;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var tiempoPrendida = DateTime.UtcNow - Inicio;
            var descripcion = "API operativa, prendida hace " + tiempoPrendida.ToString(@"hh\:mm\:ss");
            return Task.FromResult(HealthCheckResult.Healthy(descripcion));
        }
    }
}