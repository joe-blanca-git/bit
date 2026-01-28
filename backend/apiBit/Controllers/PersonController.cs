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
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        /// <summary>
        /// Cria ou Atualiza o perfil do usuário logado.
        /// </summary>
        /// <remarks>
        /// Envie os dados pessoais e uma lista de endereços.
        /// </remarks>
        /// <response code="200">Perfil salvo com sucesso.</response>
        /// <response code="400">Retorna mensagem de erro se os dados forem inválidos.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpPost("profile")]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] PersonDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var result = await _personService.CreateOrUpdateProfile(userId, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Erro ao salvar perfil", 
                    Errors = new[] { ex.Message } 
                });
            }
        }

        /// <summary>
        /// Obtém os dados do perfil do usuário logado.
        /// </summary>
        /// <remarks>
        /// Retorna os dados pessoais e endereços do usuário identificado pelo Token.
        /// Se o usuário ainda não completou o cadastro, retorna 404.
        /// </remarks>
        /// <response code="200">Retorna o objeto Person com os dados.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        /// <response code="404">Perfil não encontrado (usuário novo sem cadastro).</response>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)] // <--- Retorna UM Person (não lista)
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var person = await _personService.GetProfileByUserId(userId);

            if (person == null)
            {
                return NotFound(new ErrorResponseDto 
                { 
                    Message = "Perfil não encontrado.", 
                    Errors = new[] { "Complete seu cadastro usando o método POST primeiro." } 
                });
            }

            return Ok(person);
        }
    }
}