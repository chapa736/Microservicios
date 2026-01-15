using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class RegisterResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
    }

    public class CreateUserRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UpdateUserRequest
    {
        [MaxLength(50)]
        public string Username { get; set; }
        
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        
        public bool? Activo { get; set; }
    }

    public class CreateRoleRequest
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }
        
        [MaxLength(150)]
        public string Descripcion { get; set; }
    }
}