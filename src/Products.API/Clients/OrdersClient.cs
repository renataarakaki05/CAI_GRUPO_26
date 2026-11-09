using System.Net.Http.Json;

namespace Products.API.Clients
{
    // Cliente HTTP hacia Orders.API. Se usa para la regla PRD-004:
    // no se puede eliminar un producto que está en órdenes Pendiente o Confirmada.
    public class OrdersClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<OrdersClient> _logger;

        public OrdersClient(HttpClient http, ILogger<OrdersClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<bool> TieneOrdenesActivasAsync(Guid productoId, CancellationToken ct = default)
        {
            HttpResponseMessage response;
            try
            {
                response = await _http.GetAsync("/api/orders", ct);
            }
            catch (HttpRequestException ex)
            {
                // Mientras Orders.API no esté levantada no bloqueamos el borrado
                _logger.LogWarning(ex, "No se pudo contactar a Orders.API; se asume que el producto {ProductoId} no tiene órdenes activas", productoId);
                return false;
            }

            // Verificar siempre el status antes de leer el body
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Orders.API respondió {StatusCode}; se asume que el producto {ProductoId} no tiene órdenes activas",
                    (int)response.StatusCode, productoId);
                return false;
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(ct);
            if (orders == null)
            {
                return false;
            }

            foreach (var order in orders)
            {
                if (!EsEstadoActivo(order.Estado))
                {
                    continue;
                }

                foreach (var item in order.Items)
                {
                    if (item.ProductoId == productoId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool EsEstadoActivo(string estado)
        {
            return estado.Equals("Pendiente", StringComparison.OrdinalIgnoreCase)
                || estado.Equals("Confirmada", StringComparison.OrdinalIgnoreCase);
        }

        // Solo los campos de la orden que necesitamos
        private class OrderDto
        {
            public string Estado { get; set; } = string.Empty;
            public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        }

        private class OrderItemDto
        {
            public Guid ProductoId { get; set; }
        }
    }
}
