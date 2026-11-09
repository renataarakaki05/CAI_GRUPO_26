using System.Net;

namespace Orders.API.Clients
{
    public class UsersClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<UsersClient> _logger;

        public UsersClient(HttpClient http, ILogger<UsersClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<bool> ExisteUsuarioAsync(Guid usuarioId)
        {
            // Si Users.API no responde la excepción llega al GlobalExceptionHandler ORD-007
            var response = await _http.GetAsync($"/api/users/{usuarioId}");

            // Siempre se revisa el status antes de leer la respuesta
            if (response.IsSuccessStatusCode) return true;
            if (response.StatusCode == HttpStatusCode.NotFound) return false;

            _logger.LogError("Users.API respondió {StatusCode} al buscar el usuario {UsuarioId}",
                (int)response.StatusCode, usuarioId);
            throw new HttpRequestException($"Users.API respondió {(int)response.StatusCode}");
        }
    }
}
