using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Seguros.Core.Commands;
using Seguros.Core.Queries;
using Seguros.Core.DTOs;

namespace Seguros.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener todos los clientes (ADMINISTRADOR)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllClientesQuery();
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Obtener cliente por UserID
        /// </summary>
        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var query = new GetClienteByUserIdQuery { userId = userId };
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : NotFound(result);
        }
        /// <summary>
        /// Obtener cliente por UserID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetClienteByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Buscar cliente por identificación (ADMINISTRADOR)
        /// </summary>
        [HttpGet("identificacion/{numeroIdentificacion}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetByIdentificacion(string numeroIdentificacion)
        {
            var query = new GetClienteByIdentificacionQuery { NumeroIdentificacion = numeroIdentificacion };
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Crear cliente (Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
        {
            var command = new CreateClienteCommand
            {
                NumeroIdentificacion = dto.NumeroIdentificacion,
                Nombre = dto.Nombre,
                ApPaterno = dto.ApPaterno,
                ApMaterno = dto.ApMaterno,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion
            };

            var result = await _mediator.Send(command);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result) : BadRequest(result);
        }

        /// <summary>
        /// Actualizar cliente (Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateClienteDto dto)
        {
            var command = new UpdateClienteCommand
            {
                Id = id,
                Nombre = dto.Nombre,
                ApPaterno = dto.ApPaterno,
                ApMaterno = dto.ApMaterno,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion
            };

            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Editar información personal (Cliente)
        /// </summary>
        [HttpPatch("UpdateMyInfo")]
        [Authorize(Roles = "CLIENTE")]
        public async Task<IActionResult> UpdateMyInfo([FromBody] UpdateClienteDto dto)
        {
            
            var command = new UpdateClienteCommand
            {
                Id = dto.IdCliente,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono
            };

            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Eliminar cliente (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteClienteCommand { Id = id };
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
