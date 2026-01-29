using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        /// <summary>
        /// Lista todos os endereços do usuário logado.
        /// </summary>
        /// <response code="200">Retorna a lista de endereços.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<PersonAddress>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var addresses = await _addressService.GetAll(userId);
            return Ok(addresses);
        }

        /// <summary>
        /// Obtém um endereço específico pelo ID.
        /// </summary>
        /// <param name="id">ID do endereço</param>
        /// <response code="200">Retorna os dados do endereço.</response>
        /// <response code="404">Endereço não encontrado ou não pertence ao usuário.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PersonAddress), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var address = await _addressService.GetById(userId, id);
            
            if (address == null) 
                return NotFound(new ErrorResponseDto { Message = "Endereço não encontrado." });

            return Ok(address);
        }

        /// <summary>
        /// Cadastra um novo endereço para o usuário.
        /// </summary>
        /// <param name="model">Dados do endereço</param>
        /// <response code="201">Endereço criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpPost]
        [ProducesResponseType(typeof(PersonAddress), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] AddressDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var newAddress = await _addressService.Add(userId, model);
                return CreatedAtAction(nameof(GetById), new { id = newAddress.Id }, newAddress);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Erro ao criar endereço", 
                    Errors = new[] { ex.Message } 
                });
            }
        }

        /// <summary>
        /// Atualiza parcialmente um endereço existente.
        /// </summary>
        /// <param name="id">ID do endereço</param>
        /// <param name="model">Novos dados do endereço</param>
        /// <response code="200">Endereço atualizado com sucesso.</response>
        /// <response code="404">Endereço não encontrado.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(PersonAddress), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AddressDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var updatedAddress = await _addressService.Update(userId, id, model);
            
            if (updatedAddress == null) 
                return NotFound(new ErrorResponseDto { Message = "Endereço não encontrado ou não pertence a você." });

            return Ok(updatedAddress);
        }

        /// <summary>
        /// Exclui um endereço.
        /// </summary>
        /// <param name="id">ID do endereço a ser excluído</param>
        /// <response code="204">Endereço excluído com sucesso (sem conteúdo).</response>
        /// <response code="404">Endereço não encontrado.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _addressService.Delete(userId, id);
            
            if (!success) 
                return NotFound(new ErrorResponseDto { Message = "Endereço não encontrado." });

            return NoContent();
        }
    }
}