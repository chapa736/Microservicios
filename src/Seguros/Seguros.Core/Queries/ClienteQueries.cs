using MediatR;
using Seguros.Core.DTOs;
using Seguros.Core.Common;

namespace Seguros.Core.Queries
{
    public class GetClienteByIdQuery : IRequest<BaseResponse<ClienteDto>>
    {
        public int Id { get; set; }
    }
    public class GetClienteByUserIdQuery : IRequest<BaseResponse<ClienteDto>>
    {
        public int userId { get; set; }
    }

    public class GetClienteByIdentificacionQuery : IRequest<BaseResponse<ClienteDto>>
    {
        public string NumeroIdentificacion { get; set; }
    }

    public class GetAllClientesQuery : IRequest<BaseResponse<IEnumerable<ClienteDto>>>
    {
    }
}
