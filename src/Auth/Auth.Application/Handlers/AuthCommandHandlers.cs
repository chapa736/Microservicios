using MediatR;
using Auth.Core.Commands;
using Auth.Core.DTOs;
using Auth.Core.Common;
using Auth.Core.Interfaces.Domain;
using Auth.Core.Interfaces.Application;
using Auth.Domain.Entities;
using BCrypt.Net;
using Serilog;

namespace Auth.Application.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, BaseResponse<LoginResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<BaseResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

                if (user == null || !user.Activo || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    Log.Warning("Intento de login fallido para usuario: {Username}", request.Username);
                    return new BaseResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Credenciales inválidas"
                    };
                }

                var roles = await _unitOfWork.Roles.GetUserRolesAsync(user.Id);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Status = user.Activo ? Core.Enums.UserStatus.Activo : Core.Enums.UserStatus.Inactivo,
                    FechaCreacion = user.FechaCreacion,
                    Roles = roles.Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        Descripcion = r.Descripcion,
                        Status = r.Activo ? Core.Enums.UserStatus.Activo : Core.Enums.UserStatus.Inactivo
                    }).ToList()
                };

                var accessToken = _tokenService.GenerateAccessToken(userDto);
                var refreshToken = _tokenService.GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    FechaExp = DateTime.Now.AddDays(7)
                };

                await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                Log.Information("Login exitoso para usuario: {Username} - UserId: {UserId}", user.Username, user.Id);

                return new BaseResponse<LoginResponseDto>
                {
                    Success = true,
                    Data = new LoginResponseDto
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = DateTime.Now.AddHours(1),
                        User = userDto
                    },
                    Message = "Login exitoso"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex, "Error en login para usuario: {Username}", request.Username);
                return new BaseResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, BaseResponse<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (await _unitOfWork.Users.ExistsAsync(request.Username, request.Email))
                {
                    Log.Warning("Intento de registro con usuario o email duplicado: {Username} - {Email}", request.Username, request.Email);
                    return new BaseResponse<UserDto>
                    {
                        Success = false,
                        Message = "El usuario ya existe"
                    };
                }
                //Validar si el RoleId existe
                var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
                if (role == null || !role.Activo)
                {
                    Log.Warning("Intento de registro con rol inválido: {RoleId}", request.RoleId);
                    return new BaseResponse<UserDto>
                    {
                        Success = false,
                        Message = "El rol especificado no existe o está inactivo"
                    };
                }

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Activo = true
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                //Asignar rol al usuario
                await _unitOfWork.Roles.AddUserRoleAsync(user.Id, request.RoleId);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                Log.Information("Usuario registrado exitosamente: {Username} - UserId: {UserId} - RoleId: {RoleId}", user.Username, user.Id, request.RoleId);

                return new BaseResponse<UserDto>
                {
                    Success = true,
                    Data = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Status = Core.Enums.UserStatus.Activo,
                        FechaCreacion = user.FechaCreacion,
                        Roles = new List<RoleDto>
                        {
                            new RoleDto
                            {
                                Id = role.Id,
                                Nombre = role.Nombre,
                                Descripcion = role.Descripcion,
                                Status = role.Activo ? Core.Enums.UserStatus.Activo : Core.Enums.UserStatus.Inactivo
                            }
                        }
                    },
                    Message = "Usuario registrado exitosamente"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex, "Error al registrar usuario: {Username}", request.Username);
                return new BaseResponse<UserDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, BaseResponse<TokenDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<BaseResponse<TokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

                if (refreshToken == null || refreshToken.FechaExp < DateTime.Now || refreshToken.Revocado)
                {
                    Log.Warning("Intento de refresh con token inválido o expirado");
                    return new BaseResponse<TokenDto>
                    {
                        Success = false,
                        Message = "Token inválido o expirado"
                    };
                }

                var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
                if (user == null || !user.Activo)
                {
                    return new BaseResponse<TokenDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado o inactivo"
                    };
                }

                var roles = await _unitOfWork.Roles.GetUserRolesAsync(user.Id);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Status = Core.Enums.UserStatus.Activo,
                    FechaCreacion = user.FechaCreacion,
                    Roles = roles.Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        Descripcion = r.Descripcion,
                        Status = r.Activo ? Core.Enums.UserStatus.Activo : Core.Enums.UserStatus.Inactivo
                    }).ToList()
                };

                var newAccessToken = _tokenService.GenerateAccessToken(userDto);

                Log.Information("Token renovado exitosamente para usuario: {UserId}", user.Id);

                return new BaseResponse<TokenDto>
                {
                    Success = true,
                    Data = new TokenDto
                    {
                        AccessToken = newAccessToken,
                        ExpiresAt = DateTime.Now.AddHours(1)
                    },
                    Message = "Token renovado exitosamente"
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al renovar token");
                return new BaseResponse<TokenDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public LogoutCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

                if (refreshToken != null)
                {
                    await _unitOfWork.RefreshTokens.RevokeAsync(refreshToken.Token);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    Log.Information("Logout exitoso para usuario: {UserId}", refreshToken.UserId);
                }

                return new BaseResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Logout exitoso"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex, "Error al hacer logout");
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}