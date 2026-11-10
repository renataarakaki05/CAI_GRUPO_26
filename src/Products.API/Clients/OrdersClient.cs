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
                // Si Orders.API está caída no podemos saber si hay órdenes activas, así que NO se borra.
                // La excepción llega al GlobalExceptionHandler, que responde 500 con PRD-005.
                throw new InvalidOperationException(
                    $"No se pudo contactar a Orders.API para verificar las órdenes del producto {productoId}.", ex);
            }

            // Verificar siempre el status antes de leer el body
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Orders.API respondió {(int)response.StatusCode} al verificar las órdenes del producto {productoId}.");
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(ct);
            if (orders == null)
            {
                throw new InvalidOperationException(
                    $"Orders.API devolvió una respuesta vacía al verificar las órdenes del producto {productoId}.");
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
