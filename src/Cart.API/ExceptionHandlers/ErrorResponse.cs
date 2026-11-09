using System.Text.Json.Serialization;

namespace Cart.API.ExceptionHandlers
{
    
    // Todas las respuestas de error (4xx y 5xx) salen con esta forma.
    public class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string? Instance { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;

        // Solo se llena en Development para ver qué paso. Si está vacío no aparece en el JSON.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ExceptionDetail { get; set; }
    }
}