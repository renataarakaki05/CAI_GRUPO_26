using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks
{
    // chequea si products.api responde, xq sin products no podemos agregar al carrito
    // si esta caida devolvemos Degraded: cart anda, pero a medias
    public class ProductsApiHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public ProductsApiHealthCheck(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var url = _config["Services:ProductsApi"];
            if (url == null)
            {
                url = "http://localhost:5101";
            }

            try
            {
                var http = _httpFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(3);
                var response = await http.GetAsync(url + "/health/live", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("Products.API responde");
                }

                return HealthCheckResult.Degraded("Products.API respondio " + (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                // solo el mensaje, sin el stack trace entero
                return HealthCheckResult.Degraded("Products.API no responde: " + ex.Message);
            }
        }
    }
}