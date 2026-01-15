using MediatR;
using Auth.Core.DTOs;
using Auth.Core.Common;

namespace Auth.Core.Queries
{
    public class GetUserByIdQuery : IRequest<BaseResponse<UserDto>>
    {
        public int Id { get; set; }
    }

    public class GetUserByUsernameQuery : IRequest<BaseResponse<UserDto>>
    {
        public string Username { get; set; }
    }

    public class GetAllUsersQuery : IRequest<BaseResponse<IEnumerable<UserDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllRolesQuery : IRequest<BaseResponse<IEnumerable<RoleDto>>>
    {
    }

    public class GetUserRolesQuery : IRequest<BaseResponse<IEnumerable<RoleDto>>>
    {
        public int UserId { get; set; }
    }
}