namespace Cart.API.Models
{
    // el carrito de un usuario, cada usuario tiene uno solo
    // por eso no tiene id propio, lo buscamos por UsuarioId
    public class Carrito
    {
        // de quien es el carrito
        public Guid UsuarioId { get; set; }

        // los productos q tiene adentro, arranca vacio
        public List<CarritoItem> Items { get; set; } = new List<CarritoItem>();

        // ultima vez q se modifico
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}