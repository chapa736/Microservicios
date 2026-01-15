using MediatR;
using Auth.Core.Queries;
using Auth.Core.DTOs;
using Auth.Core.Common;
using Auth.Core.Interfaces.Domain;

namespace Auth.Application.Handlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, BaseResponse<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
                
                if (user == null)
                {
                    return new BaseResponse<UserDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                var roles = await _unitOfWork.Roles.GetUserRolesAsync(user.Id);

                return new BaseResponse<UserDto>
                {
                    Success = true,
                    Data = new UserDto
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
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserDto>
                {
                    Success = false,
                    Message = "Error al obtener usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, BaseResponse<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserByUsernameQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<UserDto>> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
                
                if (user == null)
                {
                    return new BaseResponse<UserDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                var roles = await _unitOfWork.Roles.GetUserRolesAsync(user.Id);

                return new BaseResponse<UserDto>
                {
                    Success = true,
                    Data = new UserDto
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
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserDto>
                {
                    Success = false,
                    Message = "Error al obtener usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, BaseResponse<IEnumerable<UserDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                
                var userDtos = new List<UserDto>();
                
                foreach (var user in users)
                {
                    var roles = await _unitOfWork.Roles.GetUserRolesAsync(user.Id);
                    
                    userDtos.Add(new UserDto
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
                    });
                }

                return new BaseResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Data = userDtos
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<UserDto>>
                {
                    Success = false,
                    Message = "Error al obtener usuarios",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, BaseResponse<IEnumerable<RoleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllRolesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<IEnumerable<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetAllAsync();

                return new BaseResponse<IEnumerable<RoleDto>>
                {
                    Success = true,
                    Data = roles.Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        Descripcion = r.Descripcion,
                        Status = r.Activo ? Core.Enums.UserStatus.Activo : Core.Enums.UserStatus.Inactivo
                    })
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<RoleDto>>
                {
                    Success = false,
                    Message = "Error al obtener roles",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
