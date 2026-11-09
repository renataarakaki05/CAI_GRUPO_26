using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data
{
    // crea las tablas cuando arranca la api (si ya existen no hace nada)
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
            // sacamos la conexion del appsettings, si no esta usamos cart.db
            var connectionString = _config.GetConnectionString("DefaultConnection");
            if (connectionString == null)
            {
                connectionString = "Data Source=cart.db";
            }

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // una fila por carrito (uno por usuario)
            // los guid y las fechas se guardan como texto
            connection.Execute("""
                CREATE TABLE IF NOT EXISTS carritos (
                    usuario_id          TEXT PRIMARY KEY,
                    fecha_actualizacion TEXT NOT NULL
                );
            """);

            // los productos de cada carrito
            // la clave es usuario mas producto asi el mismo producto no se repite en un carrito
            connection.Execute("""
                CREATE TABLE IF NOT EXISTS carrito_items (
                    usuario_id  TEXT    NOT NULL,
                    producto_id TEXT    NOT NULL,
                    cantidad    INTEGER NOT NULL,
                    PRIMARY KEY (usuario_id, producto_id)
                );
            """);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}