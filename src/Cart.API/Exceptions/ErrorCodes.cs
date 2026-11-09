namespace Cart.API.Exceptions
{
   
    public static class ErrorCodes
    {
        public const string CRT_001 = "CRT-001"; // 404 Carrito no encontrado
        public const string CRT_002 = "CRT-002"; // 404 Producto no encontrado en Products.API
        public const string CRT_003 = "CRT-003"; // 422 Stock insuficiente
        public const string CRT_004 = "CRT-004"; // 400 Cantidad inválida (0 o negativa)
        public const string CRT_005 = "CRT-005"; // 500 Error interno
    }
}