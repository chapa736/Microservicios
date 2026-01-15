namespace Seguros.Core.Common
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public static class ErrorMessages
    {
        public const string CLIENTE_NOT_FOUND = "Cliente no encontrado";
        public const string CLIENTE_ALREADY_EXISTS = "El cliente ya existe";
        public const string POLIZA_NOT_FOUND = "Póliza no encontrada";
        public const string INVALID_IDENTIFICACION = "Número de identificación inválido";
        public const string INVALID_EMAIL = "Email inválido";
        public const string INVALID_DATES = "Las fechas de la póliza son inválidas";
    }
}