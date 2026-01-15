using MediatR;
using Microsoft.Extensions.Configuration;
using Seguros.Core.Queries;
using Seguros.Core.DTOs;
using Seguros.Core.Common;
using Seguros.Core.Interfaces.Domain;
using Seguros.Core.Interfaces.Application;

namespace Seguros.Application.Handlers
{
    public class GetClienteByIdQueryHandler : IRequestHandler<GetClienteByIdQuery, BaseResponse<ClienteDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClienteByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<ClienteDto>> Handle(GetClienteByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(request.Id);
                
                if (cliente == null)
                {
                    return new BaseResponse<ClienteDto>
                    {
                        Success = false,
                        Message = ErrorMessages.CLIENTE_NOT_FOUND
                    };
                }

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
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ClienteDto>
                {
                    Success = false,
                    Message = "Error al obtener cliente",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetClienteByUserIdQueryHandler : IRequestHandler<GetClienteByUserIdQuery, BaseResponse<ClienteDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public GetClienteByUserIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        public async Task<BaseResponse<ClienteDto>> Handle(GetClienteByUserIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Intentar obtener del caché
                var cacheKey = $"cliente:userid:{request.userId}";
                var cachedResult = await _cacheService.GetAsync<BaseResponse<ClienteDto>>(cacheKey, cancellationToken);
                
                if (cachedResult != null && cachedResult.Success)
                {
                    return cachedResult;
                }

                // Si no está en caché, obtener de la BD
                var cliente = await _unitOfWork.Clientes.GetByUserIdAsync(request.userId);

                if (cliente == null)
                {
                    return new BaseResponse<ClienteDto>
                    {
                        Success = false,
                        Message = ErrorMessages.CLIENTE_NOT_FOUND
                    };
                }

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
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ClienteDto>
                {
                    Success = false,
                    Message = "Error al obtener cliente",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetClienteByIdentificacionQueryHandler : IRequestHandler<GetClienteByIdentificacionQuery, BaseResponse<ClienteDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetClienteByIdentificacionQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<ClienteDto>> Handle(GetClienteByIdentificacionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdentificacionAsync(request.NumeroIdentificacion);
                
                if (cliente == null)
                {
                    return new BaseResponse<ClienteDto>
                    {
                        Success = false,
                        Message = ErrorMessages.CLIENTE_NOT_FOUND
                    };
                }

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
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ClienteDto>
                {
                    Success = false,
                    Message = "Error al obtener cliente",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }

    public class GetAllClientesQueryHandler : IRequestHandler<GetAllClientesQuery, BaseResponse<IEnumerable<ClienteDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public GetAllClientesQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        public async Task<BaseResponse<IEnumerable<ClienteDto>>> Handle(GetAllClientesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Intentar obtener del caché
                const string cacheKey = "clientes:all";
                var cachedResult = await _cacheService.GetAsync<BaseResponse<IEnumerable<ClienteDto>>>(cacheKey, cancellationToken);
                
                if (cachedResult != null && cachedResult.Success)
                {
                    return cachedResult;
                }

                // Si no está en caché, obtener de la BD
                var clientes = await _unitOfWork.Clientes.GetAllAsync();

                var clienteDtos = clientes.Select(c => new ClienteDto
                {
                    Id = c.Id,
                    NumeroIdentificacion = c.NumeroIdentificacion,
                    Nombre = c.Nombre,
                    ApPaterno = c.ApPaterno,
                    ApMaterno = c.ApMaterno,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Direccion = c.Direccion,
                    FechaCreacion = c.FechaCreacion,
                    FechaActualizacion = c.FechaActualizacion,
                    NombreCompleto = c.NombreCompleto
                });

                var result = new BaseResponse<IEnumerable<ClienteDto>>
                {
                    Success = true,
                    Data = clienteDtos
                };

                // Almacenar en caché
                var expirationMinutesSection = _configuration.GetSection("CacheSettings:ClienteExpirationMinutes");
                int expirationMinutes = 10;
                if (expirationMinutesSection != null && int.TryParse(expirationMinutesSection.Value, out var parsedMinutes))
                {
                    expirationMinutes = parsedMinutes;
                }
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(expirationMinutes), cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                return new BaseResponse<IEnumerable<ClienteDto>>
                {
                    Success = false,
                    Message = "Error al obtener clientes",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
