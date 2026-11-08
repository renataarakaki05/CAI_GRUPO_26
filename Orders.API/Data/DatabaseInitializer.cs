using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Data
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
                ?? "Data Source=orders.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Tabla de ordenes
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS orders (
                id                  TEXT PRIMARY KEY,
                usuario_id          TEXT NOT NULL,
                total               REAL NOT NULL,
                estado              TEXT NOT NULL,
                fecha_creacion      TEXT NOT NULL,
                fecha_actualizacion TEXT
            );
        """);

            // Tabla de items: cada fila es un producto de una orden
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS order_items (
                id              INTEGER PRIMARY KEY AUTOINCREMENT,
                orden_id        TEXT    NOT NULL,
                producto_id     TEXT    NOT NULL,
                cantidad        INTEGER NOT NULL,
                precio_unitario REAL    NOT NULL
            );
        """);

            _logger.LogInformation("SQLite inicializado correctamente {db}", connectionString);
        }
    }
}
