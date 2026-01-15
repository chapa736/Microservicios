using Microsoft.AspNetCore.Mvc;
using MediatR;
using Auth.Core.Commands;
using Auth.Core.DTOs;
using Auth.Core.Common;

namespace Auth.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse<LoginResponseDto>>> Login([FromBody] LoginDto request)
        {
            var command = new LoginCommand
            {
                Username = request.Username,
                Password = request.Password
            };
            
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResponse<UserDto>>> Register([FromBody] RegisterDto request)
        {
            var command = new RegisterCommand
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                RoleId = request.RoleId
            };
            
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<BaseResponse<TokenDto>>> RefreshToken([FromBody] string refreshToken)
        {
            var command = new RefreshTokenCommand { RefreshToken = refreshToken };
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("logout")]
        public async Task<ActionResult<BaseResponse<bool>>> Logout([FromBody] string refreshToken)
        {
            var command = new LogoutCommand { RefreshToken = refreshToken };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}