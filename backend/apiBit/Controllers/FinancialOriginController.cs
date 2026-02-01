using apiBit.Data;
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
    public class FinancialOriginController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FinancialOriginController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<Guid> GetCurrentCompanyId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var company = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);
            return company?.Id ?? Guid.Empty;
        }

        /// <summary>
        /// Lista todas as origens de movimentação.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyId = await GetCurrentCompanyId();
            var origins = await _context.FinancialOrigins
                                        .Where(c => c.CompanyId == companyId)
                                        .Select(o => new FinancialOriginResponseDto
                                        {
                                            Id = o.Id,
                                            Description = o.Description,
                                            Active = o.Active
                                        })
                                        .ToListAsync();
            return Ok(origins);
        }

        /// <summary>
        /// Cria uma nova origem.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateFinancialOriginDto model)
        {
            var companyId = await GetCurrentCompanyId();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == Guid.Empty) return BadRequest(new { message = "Empresa não encontrada." });

            var origin = new FinancialOrigin
            {
                CompanyId = companyId,
                Description = model.Description,
                Active = true,
                CreatedBy = userId ?? ""
            };

            _context.FinancialOrigins.Add(origin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = origin.Id }, origin);
        }

        /// <summary>
        /// Atualiza uma origem.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFinancialOriginDto model)
        {
            var companyId = await GetCurrentCompanyId();
            var origin = await _context.FinancialOrigins.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (origin == null) return NotFound();

            origin.Description = model.Description;
            origin.Active = model.Active;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Desativa uma origem.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var companyId = await GetCurrentCompanyId();
            var origin = await _context.FinancialOrigins.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (origin == null) return NotFound();

            origin.Active = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}