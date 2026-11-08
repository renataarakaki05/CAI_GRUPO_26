using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Data
{
    public class OrdenRepository
    {
        private readonly IConfiguration _config;

        public OrdenRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db");

        // GET ALL 
        public async Task<List<Orden>> GetAllAsync(Guid? usuarioId)
        {
            using var conn = CreateConnection();

            string sql = """
            SELECT id, usuario_id AS UsuarioId, total, estado,
                   fecha_creacion AS FechaCreacion
            FROM orders
        """;

            IEnumerable<Orden> ordenes;
            if (usuarioId == null)
            {
                ordenes = await conn.QueryAsync<Orden>(sql);
            }
            else
            {
                ordenes = await conn.QueryAsync<Orden>(sql + " WHERE usuario_id = @id",
                    new { id = usuarioId.Value });
            }

            var lista = ordenes.ToList();
            foreach (var orden in lista)
            {
                orden.Items = await GetItemsAsync(conn, orden.Id);
            }

            return lista;
        }

        // GET BY ID
        public async Task<Orden?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            var orden = await conn.QuerySingleOrDefaultAsync<Orden>("""
            SELECT id, usuario_id AS UsuarioId, total, estado,
                   fecha_creacion AS FechaCreacion
            FROM orders
            WHERE id = @id
        """, new { id });

            if (orden != null)
            {
                orden.Items = await GetItemsAsync(conn, orden.Id);
            }

            return orden;
        }

        // Trae los productos de una orden
        private static async Task<List<OrdenItem>> GetItemsAsync(SqliteConnection conn, Guid ordenId)
        {
            var items = await conn.QueryAsync<OrdenItem>("""
            SELECT producto_id AS ProductoId, cantidad,
                   precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE orden_id = @ordenId
        """, new { ordenId });

            return items.ToList();
        }

        // CREATE 
        public async Task CreateAsync(Orden orden)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            await conn.ExecuteAsync("""
            INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
            VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)
        """, orden, transaction);

            foreach (var item in orden.Items)
            {
                await conn.ExecuteAsync("""
                INSERT INTO order_items (orden_id, producto_id, cantidad, precio_unitario)
                VALUES (@OrdenId, @ProductoId, @Cantidad, @PrecioUnitario)
            """, new { OrdenId = orden.Id, item.ProductoId, item.Cantidad, item.PrecioUnitario }, transaction);
            }

            transaction.Commit();
        }

        // UPDATE del estado
        public async Task UpdateEstadoAsync(Guid id, string estado, DateTime fechaActualizacion)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
            UPDATE orders
            SET estado              = @estado,
                fecha_actualizacion = @fechaActualizacion
            WHERE id = @id
        """, new { id, estado, fechaActualizacion });
        }
    }
}
