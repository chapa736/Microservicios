using MediatR;
using Seguros.Core.DTOs;
using Seguros.Core.Common;
using Seguros.Core.Enums;

namespace Seguros.Core.Commands
{
    public class CreatePolizaCommand : IRequest<BaseResponse<PolizaDto>>
    {
        public int IdCliente { get; set; }
        public TipoPoliza TipoPoliza { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Monto { get; set; }
        public EstatusPoliza Estatus { get; set; }
    }

    public class UpdatePolizaCommand : IRequest<BaseResponse<PolizaDto>>
    {
        public int Id { get; set; }
        public DateTime? FechaFin { get; set; }
        public decimal? Monto { get; set; }
        public EstatusPoliza? Estatus { get; set; }
    }

    public class CancelarPolizaCommand : IRequest<BaseResponse<bool>>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
