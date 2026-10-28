using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Data
{
    //  basada en el ItemRepository (Dapper + SQLite).
    public class ProductoRepository
    {
        private readonly IConfiguration _config;

        public ProductoRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=products.db");

        private const string SelectColumns = """
            SELECT id, nombre, descripcion, precio, stock, categoria,
                   fecha_creacion AS FechaCreacion
            FROM products
        """;

        // ── GET ALL ───────────────────────────────────────────────────────────
        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            using var conn = CreateConnection();
            var rows = await conn.QueryAsync<ProductRow>($"{SelectColumns} ORDER BY fecha_creacion DESC");
            return rows.Select(r => r.ToEntity());
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<Producto?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            var row = await conn.QuerySingleOrDefaultAsync<ProductRow>(
                $"{SelectColumns} WHERE id = @id", new { id = id.ToString() });
            return row?.ToEntity();
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task CreateAsync(Producto Producto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
            INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fecha_creacion)
            VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
        """, ToParameters(Producto));
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(Producto Producto)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("""
            UPDATE products
            SET nombre      = @Nombre,
                descripcion = @Descripcion,
                precio      = @Precio,
                stock       = @Stock,
                categoria   = @Categoria
            WHERE id = @Id
        """, ToParameters(Producto));

            return rows > 0;
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(Guid id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("DELETE FROM products WHERE id = @id", new { id = id.ToString() });
            return rows > 0;
        }

        private static object ToParameters(Producto p) => new
        {
            Id = p.Id.ToString(),
            p.Nombre,
            p.Descripcion,
            Precio = (double)p.Precio,
            p.Stock,
            p.Categoria,
            FechaCreacion = p.FechaCreacion.ToString("o", CultureInfo.InvariantCulture)
        };

        // Fila tal como viene de SQLite (Guid y fecha como texto)
        private sealed class ProductRow
        {
            public string Id { get; init; } = string.Empty;
            public string Nombre { get; init; } = string.Empty;
            public string? Descripcion { get; init; }
            public double Precio { get; init; }
            public long Stock { get; init; }
            public string Categoria { get; init; } = string.Empty;
            public string FechaCreacion { get; init; } = string.Empty;

            public Producto ToEntity() => new()
            {
                Id = Guid.Parse(Id),
                Nombre = Nombre,
                Descripcion = Descripcion,
                Precio = (decimal)Precio,
                Stock = (int)Stock,
                Categoria = Categoria,
                FechaCreacion = DateTime.Parse(FechaCreacion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };
        }
    }
}
