namespace Orders.API.Exceptions;

public static class ErrorCodes
{
    public const string ORD_001 = "ORD-001"; // 404 orden no encontrada
    public const string ORD_002 = "ORD-002"; // 400 datos de la orden inválidos
    public const string ORD_003 = "ORD-003"; // 404 usuario no encontrado al crear la orden
    public const string ORD_004 = "ORD-004"; // 404 producto no encontrado al crear la orden
    public const string ORD_005 = "ORD-005"; // 422 stock insuficiente
    public const string ORD_006 = "ORD-006"; // 409 el estado de la orden no puede ser modificado
    public const string ORD_007 = "ORD-007"; // 500 error interno
}
