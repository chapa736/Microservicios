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
    public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, BaseResponse<ClienteDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateClienteCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<ClienteDto>> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (await _unitOfWork.Clientes.ExistsAsync(request.NumeroIdentificacion))
                {
                    throw new DuplicateException($"Ya existe un cliente con el número de identificación {request.NumeroIdentificacion}");
                }

                var cliente = new Cliente
                {
                    NumeroIdentificacion = request.NumeroIdentificacion,
                    Nombre = request.Nombre,
                    ApPaterno = request.ApPaterno,
                    ApMaterno = request.ApMaterno,
                    Telefono = request.Telefono,
                    Email = request.Email,
                    Direccion = request.Direccion,
                    UserId = request.UserId
                };

                await _unitOfWork.Clientes.AddAsync(cliente);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                Log.Information("Cliente creado exitosamente: {ClienteId} - {NumeroIdentificacion}", cliente.Id, cliente.NumeroIdentificacion);

                return new BaseResponse<ClienteDto>
                {
                    Success = true,
                    Data = new ClienteDto
                    {
                        Id = cliente.Id,
                        NumeroIdentificacion = cliente.NumeroIdentificacion,
                        Nombre = cliente.Nombre,
                        ApPaterno = cliente.ApPaterno,
                        ApMaterno = cliente.ApMaterno,
                        Telefono = cliente.Telefono,
                        Email = cliente.Email,
                        Direccion = cliente.Direccion,
                        FechaCreacion = cliente.FechaCreacion,
                        NombreCompleto = cliente.NombreCompleto
                    },
                    Message = "Cliente creado exitosamente"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex, "Error al crear cliente. NumeroIdentificacion: {NumeroIdentificacion}", request.NumeroIdentificacion);
                return new BaseResponse<ClienteDto>
                {
                    Success = false,
                    Message = "Error al crear cliente",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, BaseResponse<ClienteDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateClienteCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<ClienteDto>> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cliente = await _unitOfWork.Clientes.GetByIdAsync(request.Id);
                
                if (cliente == null)
                {
                    throw new NotFoundException($"Cliente con ID {request.Id} no encontrado");
                }

                if (!string.IsNullOrEmpty(request.Nombre))
                    cliente.Nombre = request.Nombre;
                if (!string.IsNullOrEmpty(request.ApPaterno))
                    cliente.ApPaterno = request.ApPaterno;
                if (!string.IsNullOrEmpty(request.ApMaterno))
                    cliente.ApMaterno = request.ApMaterno;
                if (!string.IsNullOrEmpty(request.Telefono))
                    cliente.Telefono = request.Telefono;
                if (!string.IsNullOrEmpty(request.Email))
                    cliente.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Direccion))
                    cliente.Direccion = request.Direccion;

                cliente.FechaActualizacion = DateTime.Now;

                await _unitOfWork.Clientes.UpdateAsync(cliente);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                Log.Information("Cliente actualizado exitosamente: {ClienteId}", cliente.Id);

                return new BaseResponse<ClienteDto>
                {
                    Success = true,
                    Data = new ClienteDto
                    {
                        Id = cliente.Id,
                        NumeroIdentificacion = cliente.NumeroIdentificacion,
                        Nombre = cliente.Nombre,
                        ApPaterno = cliente.ApPaterno,
                        ApMaterno = cliente.ApMaterno,
                        Telefono = cliente.Telefono,
                        Email = cliente.Email,
                        Direccion = cliente.Direccion,
                        FechaCreacion = cliente.FechaCreacion,
                        FechaActualizacion = cliente.FechaActualizacion,
                        NombreCompleto = cliente.NombreCompleto
                    },
                    Message = "Cliente actualizado exitosamente"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex, "Error al actualizar cliente. ClienteId: {ClienteId}", request.Id);
                return new BaseResponse<ClienteDto>
                {
                    Success = false,
                    Message = "Error al actualizar cliente",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class DeleteClienteCommandHandler : IRequestHandler<DeleteClienteCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteClienteCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<bool>> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cliente = await _unitOfWork.Clientes.GetByIdAsync(request.Id);
                
                if (cliente == null)
                {
                    throw new NotFoundException($"Cliente con ID {request.Id} no encontrado");
                }

                await _unitOfWork.Clientes.DeleteAsync(request.Id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                Log.Information("Cliente eliminado exitosamente: {ClienteId}", request.Id);

                return new BaseResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Cliente eliminado exitosamente"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                Log.Error(ex, "Error al eliminar cliente. ClienteId: {ClienteId}", request.Id);
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Error al eliminar cliente",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
