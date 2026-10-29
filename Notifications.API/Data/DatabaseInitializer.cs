using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data
{
    // Basado en el DatabaseInitializer de la cátedra (MiniApi), adaptado a la tabla notifications.
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=notifications.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Los Guid se guardan como TEXT; la fecha se guarda en formato ISO 8601
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS notifications (
                id          TEXT PRIMARY KEY,
                usuario_id  TEXT NOT NULL,
                mensaje     TEXT NOT NULL,
                tipo        TEXT NOT NULL,
                estado      TEXT NOT NULL,
                fecha_envio TEXT NOT NULL
            );
        """);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}
