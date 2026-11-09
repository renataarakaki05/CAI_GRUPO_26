using System.Net;

namespace Orders.API.Clients
{
    // Le pide a Products.API los datos de un producto
    public class ProductsClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ProductsClient> _logger;

        public ProductsClient(HttpClient http, ILogger<ProductsClient> logger)
        {
            _http = http;
            _logger = logger;
        }
        public async Task<ProductoInfo?> GetProductoAsync(Guid productoId) // Devuelve null si el producto no existe
        {
            // Si Products.API no responde, la excepción llega al GlobalExceptionHandler (ORD-007)
            var response = await _http.GetAsync($"/api/products/{productoId}");

            // Siempre se revisa el status antes de leer la respuesta
            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Products.API respondió {StatusCode} al buscar el producto {ProductoId}",
                    (int)response.StatusCode, productoId);
                throw new HttpRequestException($"Products.API respondió {(int)response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<ProductoInfo>();
        }
    }

    // Solo los datos del producto que necesita Orders.API
    public record ProductoInfo(
        Guid Id,
        string Nombre,
        decimal Precio,
        int Stock
    );
}
