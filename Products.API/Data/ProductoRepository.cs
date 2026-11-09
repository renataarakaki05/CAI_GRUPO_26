using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Data
{

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


        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            using var conn = CreateConnection();
            var rows = await conn.QueryAsync<ProductRow>($"{SelectColumns} ORDER BY fecha_creacion DESC");

            var productos = new List<Producto>();
            foreach (var row in rows)
            {
                productos.Add(row.ToEntity());
            }

            return productos;
        }


        public async Task<Producto?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            var row = await conn.QuerySingleOrDefaultAsync<ProductRow>(
                $"{SelectColumns} WHERE id = @id", new { id = id.ToString() });
            if (row == null)
            {
                return null;
            }

            return row.ToEntity();
        }


        public async Task CreateAsync(Producto Producto)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
            INSERT INTO products (id, nombre, descripcion, precio, stock, categoria, fecha_creacion)
            VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
        """, ToParameters(Producto));
        }


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


        public async Task<bool> DeleteAsync(Guid id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("DELETE FROM products WHERE id = @id", new { id = id.ToString() });
            return rows > 0;
        }

        private static object ToParameters(Producto p)
        {
            return new
            {
                Id = p.Id.ToString(),
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = (double)p.Precio,
                Stock = p.Stock,
                Categoria = p.Categoria,
                FechaCreacion = p.FechaCreacion.ToString("o", CultureInfo.InvariantCulture)
            };
        }


        private class ProductRow
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public double Precio { get; set; }
            public long Stock { get; set; }
            public string Categoria { get; set; } = string.Empty;
            public string FechaCreacion { get; set; } = string.Empty;

            public Producto ToEntity()
            {
                var producto = new Producto();
                producto.Id = Guid.Parse(Id);
                producto.Nombre = Nombre;
                producto.Descripcion = Descripcion;
                producto.Precio = (decimal)Precio;
                producto.Stock = (int)Stock;
                producto.Categoria = Categoria;
                producto.FechaCreacion = DateTime.Parse(FechaCreacion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                return producto;
            }
        }
    }
}
