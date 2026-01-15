using MediatR;
using Seguros.Core.Commands;
using Seguros.Core.DTOs;
using Seguros.Core.Common;
using Seguros.Core.Interfaces.Domain;
using Seguros.Core.Exceptions;
using Seguros.Domain.Entities;
using Serilog;

namespace Seguros.Application.Handlers
{
    public class CreatePolizaCommandHandler : IRequestHandler<CreatePolizaCommand, BaseResponse<PolizaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreatePolizaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<PolizaDto>> Handle(CreatePolizaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validar que el cliente existe
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(request.IdCliente);
                if (cliente == null)
                {
                    throw new NotFoundException($"Cliente con ID {request.IdCliente} no encontrado");
                }

                // Validar fechas
                if (request.FechaFin <= request.FechaInicio)
                {
                    throw new ValidationException("La fecha de expiración debe ser posterior a la fecha de inicio");
                }

                // Validar monto
                if (request.Monto <= 0)
                {
                    throw new ValidationException("El monto asegurado debe ser un valor positivo");
                }

                var poliza = new Poliza
                {
                    IdCliente = request.IdCliente,
                    TipoPoliza = (int)request.TipoPoliza,
                    FechaInicio = request.FechaInicio,
                    FechaFin = request.FechaFin,
                    Monto = request.Monto,
                    Estatus = (int)request.Estatus
                };

                await _unitOfWork.Polizas.AddAsync(poliza);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                Log.Information("Póliza creada exitosamente: {PolizaId} - Cliente: {ClienteId}", poliza.Id, poliza.IdCliente);

                return new BaseResponse<PolizaDto>
                {
                    Success = true,
                    Data = new PolizaDto
                    {
                        Id = poliza.Id,
                        IdCliente = poliza.IdCliente,
                        TipoPoliza = request.TipoPoliza,
                        FechaInicio = poliza.FechaInicio,
                        FechaFin = poliza.FechaFin,
                        Monto = poliza.Monto,
                        Estatus = request.Estatus,
                        FechaCreacion = poliza.FechaCreacion,
                        EsVigente = poliza.EsVigente
                    },
                    Message = "Póliza creada exitosamente"
                };
            }
            catch (Exception ex) when (ex is not BusinessException && ex is not NotFoundException && ex is not ValidationException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex, "Error al crear póliza. ClienteId: {ClienteId}", request.IdCliente);
                return new BaseResponse<PolizaDto>
                {
                    Success = false,
                    Message = "Error al crear póliza",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class CancelarPolizaCommandHandler : IRequestHandler<CancelarPolizaCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CancelarPolizaCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<bool>> Handle(CancelarPolizaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var poliza = await _unitOfWork.Polizas.GetByIdAsync(request.Id);
                
                if (poliza == null)
                {
                    throw new NotFoundException($"Póliza con ID {request.Id} no encontrada");
                }

                // Verificar que la póliza pertenece al cliente
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(poliza.IdCliente);
                if (cliente.UserId != request.UserId)
                {
                    throw new BusinessException("No tiene permisos para cancelar esta póliza");
                }

                // Verificar que la póliza está activa
                if (poliza.Estatus != (int)Core.Enums.EstatusPoliza.Vigente)
                {
                    throw new BusinessException("Solo se pueden cancelar pólizas activas");
                }

                poliza.Estatus = (int)Core.Enums.EstatusPoliza.Cancelada;
                await _unitOfWork.Polizas.UpdateAsync(poliza);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                Log.Information("Póliza cancelada exitosamente: {PolizaId} - Usuario: {UserId}", request.Id, request.UserId);

                return new BaseResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Póliza cancelada exitosamente"
                };
            }
            catch (Exception ex) when (ex is not BusinessException && ex is not NotFoundException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex, "Error al cancelar póliza. PolizaId: {PolizaId}, UserId: {UserId}", request.Id, request.UserId);
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Error al cancelar póliza",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
