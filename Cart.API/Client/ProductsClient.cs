using System.Net;
using System.Net.Http.Json;

namespace Cart.API.Clients
{
    // le pega a products.api para saber si un producto existe y cuanto stock tiene
    public class ProductsClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ProductsClient> _logger;

        public ProductsClient(HttpClient http, IConfiguration config, ILogger<ProductsClient> logger)
        {
            _http = http;
            _logger = logger;

            // la direccion de products sale del appsettings 
            var url = config["Services:ProductsApi"];
            if (url == null)
            {
                url = "http://localhost:5101";
            }

            _http.BaseAddress = new Uri(url);
            // si products tarda mas de 5 seg cortamos
            _http.Timeout = TimeSpan.FromSeconds(5);
        }

        // busca el producto en products, si no existe devuelve null
        public async Task<ProductoInfo?> GetProductoAsync(Guid productoId, CancellationToken ct = default)
        {
            // si products esta apagado salta una excepcion aca y termina en CRT-005
            var response = await _http.GetAsync($"/api/products/{productoId}", ct);

            // primero miramos el codigo de la respuesta, despues leemos el body
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Products.API respondio {StatusCode} al buscar el producto {ProductoId}",
                    (int)response.StatusCode, productoId);
                throw new HttpRequestException($"Products.API respondio {(int)response.StatusCode}");
            }

            var producto = await response.Content.ReadFromJsonAsync<ProductoInfo>(ct);
            return producto;
        }
    }

    // los datos del producto q nos importan (products devuelve mas cosas pero no las usamos)
    public class ProductoInfo
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}