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
        /// Obtener cliente por ID (ADMINISTRADOR)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
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
        [HttpPatch("mi-informacion")]
        [Authorize(Roles = "CLIENTE")]
        public async Task<IActionResult> UpdateMyInfo([FromBody] UpdateClienteDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Buscar cliente por UserId
            var query = new GetAllClientesQuery();
            var clientes = await _mediator.Send(query);
            var cliente = clientes.Data?.FirstOrDefault(c => c.Id == userId);
            
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            var command = new UpdateClienteCommand
            {
                Id = cliente.Id,
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
