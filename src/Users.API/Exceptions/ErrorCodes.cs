namespace Users.API.Exceptions;
public static class ErrorCodes
{
    public const string USR_001 = "USR-001"; // 409 El email ya está registrado
    public const string USR_002 = "USR-002"; // 400 Datos del usuario inválidos
    public const string USR_003 = "USR-003"; // 401 Credenciales incorrectas
    public const string USR_004 = "USR-004"; // 403 Bloqueado por intentos fallidos
    public const string USR_005 = "USR-005"; // 403 Bloqueado por fraude
    public const string USR_006 = "USR-006"; // 500 Error interno
    public const string USR_007 = "USR-007"; // 404 Usuario no encontrado
}