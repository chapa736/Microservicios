namespace Seguros.Core.Interfaces.Application
{
    public interface IAuthApiService
    {
        Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request);
    }

    public class RegisterUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
    }

    public class RegisterUserResponse
    {
        public bool Success { get; set; }
        public UserData Data { get; set; }
        public string Message { get; set; }
    }

    public class UserData
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
