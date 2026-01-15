using System.ComponentModel.DataAnnotations;
using Auth.Core.Enums;

namespace Auth.Core.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; }
    }

    public class RegisterDto
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

        [Required]
        public int RoleId { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserStatus Status { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public UserStatus Status { get; set; }
    }

    public class TokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}