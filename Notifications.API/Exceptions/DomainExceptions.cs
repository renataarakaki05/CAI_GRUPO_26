namespace Notifications.API.Exceptions;

// Excepciones de dominio: se lanzan desde la capa de servicios y las
// atrapan los IExceptionHandler de la carpeta ExceptionHandlers.

/// <summary>El recurso pedido no existe (404).</summary>
public class NotFoundException : Exception
{
    public string ErrorCode { get; }

    public NotFoundException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>Se viola una regla de negocio, por ejemplo un duplicado (409).</summary>
public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>Los datos recibidos son inválidos (400).</summary>
public class ValidationException : Exception
{
    public string ErrorCode { get; }

    public ValidationException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
