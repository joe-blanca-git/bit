using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        /// <summary>
        /// Lista todos os planos disponíveis (Acesso: Qualquer usuário logado).
        /// </summary>
        /// <response code="200">Retorna a lista de planos.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<Plan>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _planService.GetAll();
            return Ok(result);
        }

        /// <summary>
        /// Obtém um plano pelo ID.
        /// </summary>
        /// <param name="id">ID do plano</param>
        /// <response code="200">Retorna os dados do plano.</response>
        /// <response code="404">Plano não encontrado.</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Plan), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plan = await _planService.GetById(id);
            
            if (plan == null) 
                return NotFound(new ErrorResponseDto { Message = "Plano não encontrado." });
            
            return Ok(plan);
        }

        /// <summary>
        /// Cria um novo plano (Acesso: Somente Admin ou Dev).
        /// </summary>
        /// <param name="model">Dados do novo plano</param>
        /// <response code="201">Plano criado com sucesso.</response>
        /// <response code="403">Usuário não tem permissão (Requer role Admin ou Dev).</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpPost]
        [Authorize(Roles = "Admin,Dev")]
        [ProducesResponseType(typeof(Plan), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] PlanDto model)
        {
            var plan = await _planService.Create(model);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }

        /// <summary>
        /// Atualiza um plano existente (Acesso: Somente Admin ou Dev).
        /// </summary>
        /// <param name="id">ID do plano a ser atualizado</param>
        /// <param name="model">Novos dados do plano</param>
        /// <response code="200">Plano atualizado com sucesso.</response>
        /// <response code="404">Plano não encontrado.</response>
        /// <response code="403">Usuário não tem permissão (Requer role Admin ou Dev).</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Dev")]
        [ProducesResponseType(typeof(Plan), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlanDto model)
        {
            var updatedPlan = await _planService.Update(id, model);
            
            if (updatedPlan == null) 
                return NotFound(new ErrorResponseDto { Message = "Plano não encontrado." });
            
            return Ok(updatedPlan);
        }

        /// <summary>
        /// Exclui um plano (Acesso: Somente Admin ou Dev).
        /// </summary>
        /// <param name="id">ID do plano a ser excluído</param>
        /// <response code="204">Plano excluído com sucesso.</response>
        /// <response code="404">Plano não encontrado.</response>
        /// <response code="403">Usuário não tem permissão (Requer role Admin ou Dev).</response>
        /// <response code="401">Token inválido ou não informado.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Dev")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _planService.Delete(id);
            
            if (!success) 
                return NotFound(new ErrorResponseDto { Message = "Plano não encontrado." });
            
            return NoContent();
        }
    }
}