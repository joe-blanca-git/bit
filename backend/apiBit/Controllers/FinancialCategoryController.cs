using apiBit.Data;
using apiBit.DTOs; // Para ErrorResponseDto
using apiBit.DTOs;
using apiBit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class FinancialCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FinancialCategoryController(AppDbContext context)
        {
            _context = context;
        }

        // Helper para pegar a Empresa do Usuário Logado
        private async Task<Guid> GetCurrentCompanyId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var company = await _context.Companies
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(c => c.UserId == userId);
            
            return company?.Id ?? Guid.Empty;
        }

        /// <summary>
        /// Lista todas as categorias financeiras da empresa.
        /// </summary>
        /// <remarks>
        /// Retorna apenas as categorias vinculadas à empresa do usuário logado.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FinancialCategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var companyId = await GetCurrentCompanyId();
            if (companyId == Guid.Empty) return BadRequest(new { message = "Empresa não encontrada para este usuário." });

            var categories = await _context.FinancialCategories
                                           .Where(c => c.CompanyId == companyId)
                                           .Select(c => new FinancialCategoryResponseDto
                                           {
                                               Id = c.Id,
                                               Name = c.Name,
                                               Type = c.Type,
                                               Active = c.Active
                                           })
                                           .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Cria uma nova categoria financeira (Ex: Aluguel, Salário).
        /// </summary>
        /// <param name="model">Dados da nova categoria</param>
        [HttpPost]
        [ProducesResponseType(typeof(FinancialCategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateFinancialCategoryDto model)
        {
            var companyId = await GetCurrentCompanyId();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == Guid.Empty) return BadRequest(new { message = "Empresa não encontrada." });

            var category = new FinancialCategory
            {
                CompanyId = companyId,
                Name = model.Name,
                Type = model.Type,
                Active = true,
                CreatedBy = userId ?? ""
            };

            _context.FinancialCategories.Add(category);
            await _context.SaveChangesAsync();

            var response = new FinancialCategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                Active = category.Active
            };

            return CreatedAtAction(nameof(GetAll), new { id = category.Id }, response);
        }

        /// <summary>
        /// Atualiza uma categoria existente.
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="model">Dados atualizados</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFinancialCategoryDto model)
        {
            var companyId = await GetCurrentCompanyId();

            var category = await _context.FinancialCategories
                                         .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (category == null) return NotFound(new { message = "Categoria não encontrada." });

            category.Name = model.Name;
            category.Type = model.Type;
            category.Active = model.Active;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Exclui logicamente uma categoria (Desativa).
        /// </summary>
        /// <remarks>
        /// Não apagamos do banco para manter histórico, apenas marcamos como Inativa.
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var companyId = await GetCurrentCompanyId();

            var category = await _context.FinancialCategories
                                         .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (category == null) return NotFound(new { message = "Categoria não encontrada." });

            // Soft Delete (Apenas inativa)
            category.Active = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}