namespace Cart.API.Exceptions
{
   

    /// <summary>Lo que se busca no existe (404). Ej: CRT-001, CRT-002.</summary>
    public class NotFoundException : Exception
    {
        public string ErrorCode { get; }

        public NotFoundException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>Se rompe una regla del negocio (422). Ej: CRT-003, no hay stock suficiente.</summary>
    public class BusinessRuleException : Exception
    {
        public string ErrorCode { get; }

        public BusinessRuleException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>Los datos que mandaron están mal (400). Ej: CRT-004, cantidad 0 o negativa.</summary>
    public class ValidationException : Exception
    {
        public string ErrorCode { get; }

        public ValidationException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
