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

        ///<summary>
        /// Lista todos os endereços do usuário logado.
        /// </summary>
        /// <remarks>
        /// Example Result:
        /// 
        ///     GET /api/Address
        ///     {
        ///         "id": "3fa85f64....",
        ///         "zipCode": "1400000",
        ///         "street": "Rua Abc",
        ///         "number": "100",
        ///         "complement": "Apto 10",
        ///         "city": "São Paulo",
        ///         "state": "SP",
        ///         "neighborhood": "Centro",
        ///         "personId": "3fa85f6...."
        ///     }
        /// </remarks>
        /// <param name="addressDto">Endereços do Usuário</param>
        /// <returns>Message of Success</returns>
        /// <response code="200">Dados obtidos com sucesso.</response>
        /// <response code="400">Falha na validação dos dados</response>
        /// <response code="500">Erro interno no servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<PersonAddress>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var addresses = await _addressService.GetAll(userId);
            return Ok(addresses);
        }

        // GET: api/Address/{id} (Pega um só)
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PersonAddress), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var address = await _addressService.GetById(userId, id);
            if (address == null) return NotFound(new { message = "Endereço não encontrado." });

            return Ok(address);
        }

        // POST: api/Address (Cria novo)
        [HttpPost]
        [ProducesResponseType(typeof(PersonAddress), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] AddressDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var newAddress = await _addressService.Add(userId, model);
                // Retorna 201 Created com o link para buscar o item criado
                return CreatedAtAction(nameof(GetById), new { id = newAddress.Id }, newAddress);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseDto { Message = "Erro ao criar endereço", Errors = new[] { ex.Message } });
            }
        }

        // PATCH: api/Address/{id} (Atualiza)
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(PersonAddress), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AddressDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var updatedAddress = await _addressService.Update(userId, id, model);
            
            if (updatedAddress == null) 
                return NotFound(new { message = "Endereço não encontrado ou não pertence a você." });

            return Ok(updatedAddress);
        }

        // DELETE: api/Address/{id} (Apaga)
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _addressService.Delete(userId, id);
            
            if (!success) 
                return NotFound(new { message = "Endereço não encontrado." });

            return NoContent(); // 204 = Deletado com sucesso, sem conteúdo de volta
        }
    }
}