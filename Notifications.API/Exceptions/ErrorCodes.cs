namespace Notifications.API.Exceptions;


public static class ErrorCodes
{
    public const string NTF_001 = "NTF-001"; // 404 Usuario destinatario no encontrado
    public const string NTF_002 = "NTF-002"; // 400 Datos inválidos o tipo no reconocido
    public const string NTF_003 = "NTF-003"; // 404 El usuario no tiene notificaciones
    public const string NTF_004 = "NTF-004"; // 500 Error interno
}
