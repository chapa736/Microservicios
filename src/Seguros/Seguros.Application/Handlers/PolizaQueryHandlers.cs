using MediatR;
using Seguros.Core.Queries;
using Seguros.Core.DTOs;
using Seguros.Core.Common;
using Seguros.Core.Interfaces.Domain;
using Seguros.Core.Exceptions;

namespace Seguros.Application.Handlers
{
    public class GetPolizaByIdQueryHandler : IRequestHandler<GetPolizaByIdQuery, BaseResponse<PolizaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPolizaByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<PolizaDto>> Handle(GetPolizaByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var poliza = await _unitOfWork.Polizas.GetByIdAsync(request.Id);
                
                if (poliza == null)
                {
                    throw new NotFoundException($"Póliza con ID {request.Id} no encontrada");
                }

                return new BaseResponse<PolizaDto>
                {
                    Success = true,
                    Data = MapToDto(poliza)
                };
            }
            catch (Exception ex) when (ex is not NotFoundException)
            {
                return new BaseResponse<PolizaDto>
                {
                    Success = false,
                    Message = "Error al obtener póliza",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private PolizaDto MapToDto(Domain.Entities.Poliza poliza)
        {
            return new PolizaDto
            {
                Id = poliza.Id,
                IdCliente = poliza.IdCliente,
                TipoPoliza = (Core.Enums.TipoPoliza)poliza.TipoPoliza,
                FechaInicio = poliza.FechaInicio,
                FechaFin = poliza.FechaFin,
                Monto = poliza.Monto,
                Estatus = (Core.Enums.EstatusPoliza)poliza.Estatus,
                FechaCreacion = poliza.FechaCreacion,
                EsVigente = poliza.EsVigente
            };
        }
    }

    public class GetPolizasByClienteQueryHandler : IRequestHandler<GetPolizasByClienteQuery, BaseResponse<IEnumerable<PolizaDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPolizasByClienteQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<IEnumerable<PolizaDto>>> Handle(GetPolizasByClienteQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var polizas = await _unitOfWork.Polizas.GetByClienteIdAsync(request.ClienteId);

                return new BaseResponse<IEnumerable<PolizaDto>>
                {
                    Success = true,
                    Data = polizas.Select(p => new PolizaDto
                    {
                        Id = p.Id,
                        IdCliente = p.IdCliente,
                        TipoPoliza = (Core.Enums.TipoPoliza)p.TipoPoliza,
                        FechaInicio = p.FechaInicio,
                        FechaFin = p.FechaFin,
                        Monto = p.Monto,
                        Estatus = (Core.Enums.EstatusPoliza)p.Estatus,
                        FechaCreacion = p.FechaCreacion,
                        EsVigente = p.EsVigente
                    })
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<PolizaDto>>
                {
                    Success = false,
                    Message = "Error al obtener pólizas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetPolizasByTipoQueryHandler : IRequestHandler<GetPolizasByTipoQuery, BaseResponse<IEnumerable<PolizaDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPolizasByTipoQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<IEnumerable<PolizaDto>>> Handle(GetPolizasByTipoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var polizas = await _unitOfWork.Polizas.GetByTipoPolizaAsync((int)request.TipoPoliza);

                return new BaseResponse<IEnumerable<PolizaDto>>
                {
                    Success = true,
                    Data = polizas.Select(p => new PolizaDto
                    {
                        Id = p.Id,
                        IdCliente = p.IdCliente,
                        TipoPoliza = (Core.Enums.TipoPoliza)p.TipoPoliza,
                        FechaInicio = p.FechaInicio,
                        FechaFin = p.FechaFin,
                        Monto = p.Monto,
                        Estatus = (Core.Enums.EstatusPoliza)p.Estatus,
                        FechaCreacion = p.FechaCreacion,
                        EsVigente = p.EsVigente
                    })
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<PolizaDto>>
                {
                    Success = false,
                    Message = "Error al obtener pólizas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetPolizasByEstatusQueryHandler : IRequestHandler<GetPolizasByEstatusQuery, BaseResponse<IEnumerable<PolizaDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPolizasByEstatusQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<IEnumerable<PolizaDto>>> Handle(GetPolizasByEstatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var polizas = await _unitOfWork.Polizas.GetByEstatusAsync((int)request.Estatus);

                return new BaseResponse<IEnumerable<PolizaDto>>
                {
                    Success = true,
                    Data = polizas.Select(p => new PolizaDto
                    {
                        Id = p.Id,
                        IdCliente = p.IdCliente,
                        TipoPoliza = (Core.Enums.TipoPoliza)p.TipoPoliza,
                        FechaInicio = p.FechaInicio,
                        FechaFin = p.FechaFin,
                        Monto = p.Monto,
                        Estatus = (Core.Enums.EstatusPoliza)p.Estatus,
                        FechaCreacion = p.FechaCreacion,
                        EsVigente = p.EsVigente
                    })
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<PolizaDto>>
                {
                    Success = false,
                    Message = "Error al obtener pólizas",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetPolizasVigentesQueryHandler : IRequestHandler<GetPolizasVigentesQuery, BaseResponse<IEnumerable<PolizaDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPolizasVigentesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<IEnumerable<PolizaDto>>> Handle(GetPolizasVigentesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var polizas = await _unitOfWork.Polizas.GetPolizasVigentesAsync();

                return new BaseResponse<IEnumerable<PolizaDto>>
                {
                    Success = true,
                    Data = polizas.Select(p => new PolizaDto
                    {
                        Id = p.Id,
                        IdCliente = p.IdCliente,
                        TipoPoliza = (Core.Enums.TipoPoliza)p.TipoPoliza,
                        FechaInicio = p.FechaInicio,
                        FechaFin = p.FechaFin,
                        Monto = p.Monto,
                        Estatus = (Core.Enums.EstatusPoliza)p.Estatus,
                        FechaCreacion = p.FechaCreacion,
                        EsVigente = p.EsVigente
                    })
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<PolizaDto>>
                {
                    Success = false,
                    Message = "Error al obtener pólizas vigentes",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
