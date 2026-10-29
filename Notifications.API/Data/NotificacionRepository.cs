using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data
{
    // Persistencia basada en el ItemRepository de la cátedra (Dapper + SQLite).
    public class NotificacionRepository
    {
        private readonly IConfiguration _config;

        public NotificacionRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=notifications.db");

        // ── GET BY USUARIO ────────────────────────────────────────────────────
        public async Task<IEnumerable<Notificacion>> GetByUsuarioAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();
            var rows = await conn.QueryAsync<NotificacionRow>("""
            SELECT id, usuario_id AS UsuarioId, mensaje, tipo, estado, fecha_envio AS FechaEnvio
            FROM notifications
            WHERE usuario_id = @usuarioId
            ORDER BY fecha_envio DESC
        """, new { usuarioId = usuarioId.ToString() });

            return rows.Select(r => r.ToEntity());
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task CreateAsync(Notificacion notificacion)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
            INSERT INTO notifications (id, usuario_id, mensaje, tipo, estado, fecha_envio)
            VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio)
        """, new
            {
                Id = notificacion.Id.ToString(),
                UsuarioId = notificacion.UsuarioId.ToString(),
                notificacion.Mensaje,
                notificacion.Tipo,
                notificacion.Estado,
                FechaEnvio = notificacion.FechaEnvio.ToString("o", CultureInfo.InvariantCulture)
            });
        }

        // Fila tal como viene de SQLite (Guid y fecha como texto)
        private sealed class NotificacionRow
        {
            public string Id { get; init; } = string.Empty;
            public string UsuarioId { get; init; } = string.Empty;
            public string Mensaje { get; init; } = string.Empty;
            public string Tipo { get; init; } = string.Empty;
            public string Estado { get; init; } = string.Empty;
            public string FechaEnvio { get; init; } = string.Empty;

            public Notificacion ToEntity() => new()
            {
                Id = Guid.Parse(Id),
                UsuarioId = Guid.Parse(UsuarioId),
                Mensaje = Mensaje,
                Tipo = Tipo,
                Estado = Estado,
                FechaEnvio = DateTime.Parse(FechaEnvio, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };
        }
    }
}
