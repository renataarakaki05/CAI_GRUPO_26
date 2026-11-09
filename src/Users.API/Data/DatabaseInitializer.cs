using Dapper;
using Microsoft.Data.Sqlite;

namespace Users.API.Data
{
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
            // Para que Dapper sepa leer los guid guardados como texto
            SqlMapper.AddTypeHandler(new GuidTypeHandler());

            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=users.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            connection.Execute("""
            CREATE TABLE IF NOT EXISTS users (
                id                TEXT    PRIMARY KEY,
                nombre            TEXT    NOT NULL,
                apellido          TEXT    NOT NULL,
                email             TEXT    NOT NULL UNIQUE,
                password_hash     TEXT    NOT NULL,
                fecha_registro    TEXT    NOT NULL,
                activo            INTEGER NOT NULL DEFAULT 1,
                intentos_fallidos INTEGER NOT NULL DEFAULT 0
            );
        """);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}