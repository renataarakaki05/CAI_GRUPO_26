using System.Globalization;
using Cart.API.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data
{
    // aca estan todas las consultas a la base del carrito 
    public class CarritoRepository
    {
        private readonly IConfiguration _config;

        public CarritoRepository(IConfiguration config)
        {
            _config = config;
        }

        // abre una conexion nueva a la base
        private SqliteConnection CrearConexion()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            if (connectionString == null)
            {
                connectionString = "Data Source=cart.db";
            }

            return new SqliteConnection(connectionString);
        }

        // busca carrito
        // trae el carrito con sus items, si no existe devuelve null
        public async Task<Carrito?> GetByUsuarioAsync(Guid usuarioId)
        {
            using var conn = CrearConexion();

            var fila = await conn.QuerySingleOrDefaultAsync<CarritoRow>(
                "SELECT usuario_id AS UsuarioId, fecha_actualizacion AS FechaActualizacion FROM carritos WHERE usuario_id = @id",
                new { id = usuarioId.ToString() });

            if (fila == null)
            {
                return null;
            }

            var carrito = new Carrito();
            carrito.UsuarioId = Guid.Parse(fila.UsuarioId);
            carrito.FechaActualizacion = DateTime.Parse(fila.FechaActualizacion, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            // ahora buscamos los items y los vamos agregando uno por uno
            var filasItems = await conn.QueryAsync<CarritoItemRow>(
                "SELECT producto_id AS ProductoId, cantidad AS Cantidad FROM carrito_items WHERE usuario_id = @id",
                new { id = usuarioId.ToString() });

            foreach (var filaItem in filasItems)
            {
                var item = new CarritoItem();
                item.ProductoId = Guid.Parse(filaItem.ProductoId);
                item.Cantidad = (int)filaItem.Cantidad;
                carrito.Items.Add(item);
            }

            return carrito;
        }

       
        // crea el carrito vacio (se usa la primera vez q el usuario agrega algo)
        public async Task CreateAsync(Carrito carrito)
        {
            using var conn = CrearConexion();
            await conn.ExecuteAsync(
                "INSERT INTO carritos (usuario_id, fecha_actualizacion) VALUES (@UsuarioId, @Fecha)",
                new { UsuarioId = carrito.UsuarioId.ToString(), Fecha = FormatearFecha(carrito.FechaActualizacion) });
        }

        // actualizamos fecha
        // cada vez q se toca el carrito le cambiamos la fecha
        public async Task UpdateFechaAsync(Guid usuarioId, DateTime fecha)
        {
            using var conn = CrearConexion();
            await conn.ExecuteAsync(
                "UPDATE carritos SET fecha_actualizacion = @Fecha WHERE usuario_id = @UsuarioId",
                new { UsuarioId = usuarioId.ToString(), Fecha = FormatearFecha(fecha) });
        }

        // agregamos item
        public async Task AddItemAsync(Guid usuarioId, CarritoItem item)
        {
            using var conn = CrearConexion();
            await conn.ExecuteAsync(
                "INSERT INTO carrito_items (usuario_id, producto_id, cantidad) VALUES (@UsuarioId, @ProductoId, @Cantidad)",
                new { UsuarioId = usuarioId.ToString(), ProductoId = item.ProductoId.ToString(), Cantidad = item.Cantidad });
        }

        // modificamos cantidades
        public async Task UpdateItemAsync(Guid usuarioId, CarritoItem item)
        {
            using var conn = CrearConexion();
            await conn.ExecuteAsync(
                "UPDATE carrito_items SET cantidad = @Cantidad WHERE usuario_id = @UsuarioId AND producto_id = @ProductoId",
                new { UsuarioId = usuarioId.ToString(), ProductoId = item.ProductoId.ToString(), Cantidad = item.Cantidad });
        }

        // sacamos item
        public async Task DeleteItemAsync(Guid usuarioId, Guid productoId)
        {
            using var conn = CrearConexion();
            await conn.ExecuteAsync(
                "DELETE FROM carrito_items WHERE usuario_id = @UsuarioId AND producto_id = @ProductoId",
                new { UsuarioId = usuarioId.ToString(), ProductoId = productoId.ToString() });
        }

        
        // borra todos los items, el carrito queda pero vacio
        public async Task DeleteAllItemsAsync(Guid usuarioId)
        {
            using var conn = CrearConexion();
            await conn.ExecuteAsync(
                "DELETE FROM carrito_items WHERE usuario_id = @UsuarioId",
                new { UsuarioId = usuarioId.ToString() });
        }

        // la fecha se guarda como texto en formato iso (ej: 2024-03-10T10:50:00.0000000Z)
        private static string FormatearFecha(DateTime fecha)
        {
            return fecha.ToString("o", CultureInfo.InvariantCulture);
        }

        // asi vienen las filas de sqlite 
        private class CarritoRow
        {
            public string UsuarioId { get; set; } = string.Empty;
            public string FechaActualizacion { get; set; } = string.Empty;
        }

        private class CarritoItemRow
        {
            public string ProductoId { get; set; } = string.Empty;
            public long Cantidad { get; set; }
        }
    }
}