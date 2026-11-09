namespace Cart.API.Models
{
    // un producto dentro del carrito
    public class CarritoItem
    {
        // el id del producto en products.api
        public Guid ProductoId { get; set; }

        // cuantas unidades, tiene q ser mayor a 0 (si no CRT-004)
        public int Cantidad { get; set; }
    }
}