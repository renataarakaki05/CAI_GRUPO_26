using Products.API.Models;

namespace Products.API.DTOs
{

    public class ProductoDto
    {

        public Guid Id { get; set; }


        public string Nombre { get; set; } = string.Empty;


        public string? Descripcion { get; set; }


        public decimal Precio { get; set; }


        public int Stock { get; set; }


        public string Categoria { get; set; } = string.Empty;


        public DateTime FechaCreacion { get; set; }

        public static ProductoDto FromEntity(Producto p)
        {
            var dto = new ProductoDto();
            dto.Id = p.Id;
            dto.Nombre = p.Nombre;
            dto.Descripcion = p.Descripcion;
            dto.Precio = p.Precio;
            dto.Stock = p.Stock;
            dto.Categoria = p.Categoria;
            dto.FechaCreacion = p.FechaCreacion;
            return dto;
        }
    }
}
