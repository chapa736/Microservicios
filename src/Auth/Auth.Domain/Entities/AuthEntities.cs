using System.ComponentModel.DataAnnotations;

namespace Auth.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }

    public class Role
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }
        
        [MaxLength(150)]
        public string Descripcion { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }

    public class RefreshToken
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Token { get; set; }
        
        public DateTime FechaExp { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public bool Revocado { get; set; } = false;
        
        // Navigation properties
        public virtual User User { get; set; }
        
        public bool IsExpired => DateTime.Now >= FechaExp;
    }
}