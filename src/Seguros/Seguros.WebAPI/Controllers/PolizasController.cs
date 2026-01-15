using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Seguros.Core.Commands;
using Seguros.Core.Queries;
using Seguros.Core.DTOs;
using Seguros.Core.Enums;
using System.Security.Claims;

namespace Seguros.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PolizasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PolizasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener póliza por ID (Admin/Cliente)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetPolizaByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Obtener pólizas por cliente (Admin/Cliente)
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> GetByCliente(int clienteId)
        {
            var query = new GetPolizasByClienteQuery { ClienteId = clienteId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Filtrar pólizas por tipo (ADMINISTRADOR)
        /// </summary>
        [HttpGet("tipo/{tipo}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetByTipo(TipoPoliza tipo)
        {
            var query = new GetPolizasByTipoQuery { TipoPoliza = tipo };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Filtrar pólizas por estatus (ADMINISTRADOR)
        /// </summary>
        [HttpGet("estatus/{estatus}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetByEstatus(EstatusPoliza estatus)
        {
            var query = new GetPolizasByEstatusQuery { Estatus = estatus };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtener pólizas vigentes (ADMINISTRADOR)
        /// </summary>
        [HttpGet("vigentes")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetVigentes()
        {
            var query = new GetPolizasVigentesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Crear póliza (ADMINISTRADOR)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Create([FromBody] CreatePolizaDto dto)
        {
            var command = new CreatePolizaCommand
            {
                IdCliente = dto.IdCliente,
                TipoPoliza = dto.TipoPoliza,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Monto = dto.Monto,
                Estatus = dto.Estatus
            };

            var result = await _mediator.Send(command);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result) : BadRequest(result);
        }

        /// <summary>
        /// Cancelar póliza (Cliente)
        /// </summary>
        [HttpPost("{id}/cancelar")]
        [Authorize(Roles = "CLIENTE")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var command = new CancelarPolizaCommand 
            { 
                Id = id,
                UserId = userId
            };

            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
