using MediatR;
using Seguros.Core.DTOs;
using Seguros.Core.Common;
using Seguros.Core.Enums;

namespace Seguros.Core.Queries
{
    public class GetPolizaByIdQuery : IRequest<BaseResponse<PolizaDto>>
    {
        public int Id { get; set; }
    }

    public class GetPolizasByClienteQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
    {
        public int ClienteId { get; set; }
    }

    public class GetPolizasByTipoQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
    {
        public TipoPoliza TipoPoliza { get; set; }
    }

    public class GetPolizasByEstatusQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
    {
        public EstatusPoliza Estatus { get; set; }
    }

    public class GetPolizasVigentesQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
    {
    }

    public class GetPolizasByFechasQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
