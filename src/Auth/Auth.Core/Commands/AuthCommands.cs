using MediatR;
using Auth.Core.DTOs;
using Auth.Core.Common;

namespace Auth.Core.Commands
{
    public class LoginCommand : IRequest<BaseResponse<LoginResponseDto>>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterCommand : IRequest<BaseResponse<UserDto>>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
    }

    public class RefreshTokenCommand : IRequest<BaseResponse<TokenDto>>
    {
        public string RefreshToken { get; set; }
    }

    public class LogoutCommand : IRequest<BaseResponse<bool>>
    {
        public string RefreshToken { get; set; }
    }

    public class CreateUserCommand : IRequest<BaseResponse<UserDto>>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }
}