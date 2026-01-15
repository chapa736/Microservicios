using MediatR;
using Seguros.Core.DTOs;
using Seguros.Core.Common;

namespace Seguros.Core.Commands
{
    public class CreateClienteCommand : IRequest<BaseResponse<ClienteDto>>
    {
        public string NumeroIdentificacion { get; set; }
        public string Nombre { get; set; }
        public string ApPaterno { get; set; }
        public string ApMaterno { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public int? UserId { get; set; }
    }

    public class UpdateClienteCommand : IRequest<BaseResponse<ClienteDto>>
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ApPaterno { get; set; }
        public string ApMaterno { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
    }

    public class DeleteClienteCommand : IRequest<BaseResponse<bool>>
    {
        public int Id { get; set; }
    }
}
