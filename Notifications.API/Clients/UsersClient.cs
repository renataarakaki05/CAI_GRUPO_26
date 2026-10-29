using System.Net;

namespace Notifications.API.Clients
{
    // Cliente HTTP hacia Users.API. Se usa para NTF-001:
    // no se puede enviar una notificación a un usuario que no existe.
    public class UsersClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<UsersClient> _logger;

        public UsersClient(HttpClient http, ILogger<UsersClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<bool> ExisteUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        {
            // Si Users.API no responde, la excepción llega al GlobalExceptionHandler → NTF-004
            var response = await _http.GetAsync($"/api/users/{usuarioId}", ct);

            // Verificar siempre el status antes de leer el body
            if (response.IsSuccessStatusCode) return true;
            if (response.StatusCode == HttpStatusCode.NotFound) return false;

            _logger.LogError("Users.API respondió {StatusCode} al buscar el usuario {UsuarioId}",
                (int)response.StatusCode, usuarioId);
            throw new HttpRequestException($"Users.API respondió {(int)response.StatusCode}");
        }
    }
}
