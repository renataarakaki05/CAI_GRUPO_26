namespace Users.API.Exceptions;

// El recurso no existe: 404
public class NotFoundException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}

// Se rompe una regla de negocio (email repetido, usuario bloqueado, etc.)
public class BusinessRuleException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}

// Los datos que llegaron están mal: 400
public class ValidationException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
