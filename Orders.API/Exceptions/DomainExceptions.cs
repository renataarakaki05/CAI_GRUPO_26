namespace Orders.API.Exceptions;

public class NotFoundException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode; // El recurso no existe 404
}

public class BusinessRuleException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode; // Sin stock, cambio de estado no permitido
}

public class ValidationException(string errorCode, string message) : Exception(message)
{
    public string ErrorCode { get; } = errorCode; // Los datos que llegaron estan mal 400
}