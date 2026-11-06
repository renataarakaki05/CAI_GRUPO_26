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

            var notificaciones = new List<Notificacion>();
            foreach (var row in rows)
            {
                notificaciones.Add(row.ToEntity());
            }

            return notificaciones;
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
                Mensaje = notificacion.Mensaje,
                Tipo = notificacion.Tipo,
                Estado = notificacion.Estado,
                FechaEnvio = notificacion.FechaEnvio.ToString("o", CultureInfo.InvariantCulture)
            });
        }

        // Fila tal como viene de SQLite (Guid y fecha como texto)
        private class NotificacionRow
        {
            public string Id { get; set; } = string.Empty;
            public string UsuarioId { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public string FechaEnvio { get; set; } = string.Empty;

            public Notificacion ToEntity()
            {
                var notificacion = new Notificacion();
                notificacion.Id = Guid.Parse(Id);
                notificacion.UsuarioId = Guid.Parse(UsuarioId);
                notificacion.Mensaje = Mensaje;
                notificacion.Tipo = Tipo;
                notificacion.Estado = Estado;
                notificacion.FechaEnvio = DateTime.Parse(FechaEnvio, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                return notificacion;
            }
        }
    }
}
