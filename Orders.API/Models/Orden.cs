namespace Orders.API.Models;

public class Orden
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public List<OrdenItem> Items { get; set; } = new List<OrdenItem>();
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty; // pendiente, confirmada, enviada, entregada, cancelada
    public DateTime FechaCreacion { get; set; }
}