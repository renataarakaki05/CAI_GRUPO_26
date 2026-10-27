using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs
{
    /// <summary>
    /// Datos para crear (POST) o actualizar (PUT) un producto.
    /// </summary>
    public class CrearProductoDto //representa los datos para crear un prod
    {
        /// <summary>Nombre del producto. Requerido, máximo 100 caracteres.</summary>
        /// <example>Notebook Dell XPS 15</example>
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Descripción del producto. Opcional, máximo 500 caracteres.</summary>
        /// <example>Laptop 15 pulgadas, 32GB RAM</example>
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        public string? Descripcion { get; set; }

        /// <summary>Precio del producto. Debe ser mayor a 0.</summary>
        /// <example>1500.00</example>
        [Required(ErrorMessage = "El precio es requerido")]
        // ParseLimitsInInvariantCulture: sin esto, en una PC en español "0.01" no se puede leer y tira 500
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ParseLimitsInInvariantCulture = true, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal? Precio { get; set; }

        /// <summary>Unidades disponibles. Debe ser mayor o igual a 0.</summary>
        /// <example>10</example>
        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0")]
        public int? Stock { get; set; }

        /// <summary>Categoría del producto (informativa, no se valida contra una lista).</summary>
        /// <example>Electrónica</example>
        [Required(ErrorMessage = "La categoría es requerida")]
        public string Categoria { get; set; } = string.Empty;
    }
}
