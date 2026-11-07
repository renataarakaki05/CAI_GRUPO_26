namespace Cart.API.Models
{
    // Un producto dentro del carrito (en la consigna se llama CartItem).
    public class CarritoItem
    {
        // Qué producto es (el Id que tiene en Products.API)
        public Guid ProductoId { get; set; }

        // Cuántas unidades quiere. Tiene que ser mayor a 0 (eso lo chequea el servicio con CRT-004)
        public int Cantidad { get; set; }
    }
}
