namespace Auth.Core.Common
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class PaginatedResponse<T> : BaseResponse<IEnumerable<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    public static class AuthConstants
    {
        public const int ACTIVO = 1;
        public const int INACTIVO = 0;
        public const string JWT_CLAIM_USER_ID = "userId";
        public const string JWT_CLAIM_USERNAME = "username";
        public const string JWT_CLAIM_ROLES = "roles";
        public const int TOKEN_EXPIRY_MINUTES = 60;
        public const int REFRESH_TOKEN_EXPIRY_DAYS = 7;
    }

    public static class ErrorMessages
    {
        public const string INVALID_CREDENTIALS = "Credenciales inválidas";
        public const string USER_NOT_FOUND = "Usuario no encontrado";
        public const string USER_ALREADY_EXISTS = "El usuario ya existe";
        public const string INVALID_TOKEN = "Token inválido";
        public const string EXPIRED_TOKEN = "Token expirado";
        public const string ROLE_NOT_FOUND = "Rol no encontrado";
    }
}