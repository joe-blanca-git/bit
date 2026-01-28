using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Logado
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
        [HttpGet]
        [ProducesResponseType(typeof(List<Plan>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _planService.GetAll();
            return Ok(result);
        }

        /// <summary>
        /// Obtém um plano pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Plan), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plan = await _planService.GetById(id);
            if (plan == null) return NotFound(new { message = "Plano não encontrado." });
            return Ok(plan);
        }

        /// <summary>
        /// Cria um novo plano (Acesso: Somente Admin ou Dev).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Dev")] // <--- SEGURANÇA AQUI
        [ProducesResponseType(typeof(Plan), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // Sem permissão
        public async Task<IActionResult> Create([FromBody] PlanDto model)
        {
            var plan = await _planService.Create(model);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }

        /// <summary>
        /// Atualiza um plano existente (Acesso: Somente Admin ou Dev).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Dev")] // <--- SEGURANÇA AQUI
        [ProducesResponseType(typeof(Plan), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlanDto model)
        {
            var updatedPlan = await _planService.Update(id, model);
            if (updatedPlan == null) return NotFound(new { message = "Plano não encontrado." });
            
            return Ok(updatedPlan);
        }

        /// <summary>
        /// Exclui um plano (Acesso: Somente Admin ou Dev).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Dev")] // <--- SEGURANÇA AQUI
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _planService.Delete(id);
            if (!success) return NotFound(new { message = "Plano não encontrado." });
            
            return NoContent();
        }
    }
}