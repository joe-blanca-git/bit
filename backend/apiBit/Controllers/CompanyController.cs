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
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        /// <summary>
        /// Lista todas as empresas do usuário logado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<Company>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _companyService.GetAll(userId);
            return Ok(result);
        }

        /// <summary>
        /// Obtém detalhes de uma empresa específica pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Company), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var company = await _companyService.GetById(userId, id);
            if (company == null) return NotFound(new ErrorResponseDto { Message = "Empresa não encontrada." });

            return Ok(company);
        }

        /// <summary>
        /// Cadastra uma nova empresa com seus endereços.
        /// </summary>
        /// <param name="model">Dados da empresa e lista de endereços.</param>
        [HttpPost]
        [ProducesResponseType(typeof(Company), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CompanyDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var created = await _companyService.Create(userId, model);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Erro ao criar empresa", 
                    Errors = new[] { ex.Message } 
                });
            }
        }

        /// <summary>
        /// Exclui uma empresa e seus endereços.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await _companyService.Delete(userId, id);
            if (!success) return NotFound(new ErrorResponseDto { Message = "Empresa não encontrada." });

            return NoContent();
        }
    }
}