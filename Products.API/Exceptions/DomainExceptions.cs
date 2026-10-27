namespace Products.API.Exceptions;

// Excepciones de dominio: se lanzan desde la capa de servicios y las
// atrapan los IExceptionHandler de la carpeta ExceptionHandlers.

/// <summary>El recurso pedido no existe (404).</summary>
public class NotFoundException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}

/// <summary>Se viola una regla de negocio, por ejemplo un duplicado (409).</summary>
public class BusinessRuleException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}

/// <summary>Los datos recibidos son inválidos (400).</summary>
public class ValidationException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
