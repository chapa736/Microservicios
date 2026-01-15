using Auth.Core.DTOs;

namespace Auth.Core.Interfaces.Application
{
    public interface ITokenService
    {
        string GenerateAccessToken(UserDto user);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
    }
}
