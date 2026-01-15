namespace Seguros.Core.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public ValidationException(List<string> errors) : base("Errores de validación")
        {
            Errors = errors;
        }
    }

    public class DuplicateException : Exception
    {
        public DuplicateException(string message) : base(message) { }
    }
}
