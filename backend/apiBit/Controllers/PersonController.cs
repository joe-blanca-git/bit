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
        /// Se o usuário já tiver perfil, os dados serão atualizados e os endereços substituídos.
        /// </remarks>
        /// <param name="model">DTO contendo dados pessoais e endereços</param>
        /// <response code="200">Perfil salvo com sucesso.</response>
        /// <response code="400">Dados inválidos (ex: CPF já existe em outra conta).</response>
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
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
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
                    Errors = new[] { "Complete seu cadastro em POST /profile antes de realizar buscas." } 
                });
            }

            return Ok(person);
        }

        /// <summary>
        /// Lista pessoas cadastradas no sistema.
        /// </summary>
        /// <remarks>
        /// É possível filtrar pelo tipo de usuário (Role).
        /// Exemplos de filtro: CLIENT, SUPPLIER, CARRIER, OWNER, EMPLOYEE.
        /// Se o parâmetro 'type' for omitido, retorna todos.
        /// </remarks>
        /// <param name="type">Opcional: Tipo do usuário (Role)</param>
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<Person>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] string? type)
        {
            // Opcional: Se quiser restringir que CLIENTES não vejam a lista completa,
            // você pode verificar a role do usuário logado aqui.
            // Ex: if (!User.IsInRole("OWNER") && !User.IsInRole("EMPLOYEE")) return Forbidden();

            var people = await _personService.GetAllProfiles(type);
            return Ok(people);
        }
    }
}