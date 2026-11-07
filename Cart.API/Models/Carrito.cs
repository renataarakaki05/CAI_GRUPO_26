namespace Cart.API.Models
{
    // El carrito de un usuario (en la consigna se llama Cart).
    // Cada usuario tiene un solo carrito, así que no necesita Id propio: lo buscamos por UsuarioId.
    public class Carrito
    {
        // De quién es el carrito
        public Guid UsuarioId { get; set; }

        // Los productos que tiene adentro. Arranca vacío.
        public List<CarritoItem> Items { get; set; } = new List<CarritoItem>();

        // Cuándo se tocó por última vez (se actualiza en cada operación)
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}
    