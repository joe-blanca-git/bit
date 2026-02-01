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
    public class FinancialAccountController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FinancialAccountController(AppDbContext context)
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
        /// Lista todas as contas financeiras (Bancos/Caixas).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyId = await GetCurrentCompanyId();
            var accounts = await _context.FinancialAccounts
                                         .Where(c => c.CompanyId == companyId)
                                         .Select(a => new FinancialAccountResponseDto
                                         {
                                             Id = a.Id,
                                             Name = a.Name,
                                             Active = a.Active
                                         })
                                         .ToListAsync();
            return Ok(accounts);
        }

        /// <summary>
        /// Cria uma nova conta financeira.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateFinancialAccountDto model)
        {
            var companyId = await GetCurrentCompanyId();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == Guid.Empty) return BadRequest(new { message = "Empresa não encontrada." });

            var account = new FinancialAccount
            {
                CompanyId = companyId,
                Name = model.Name,
                Active = true,
                CreatedBy = userId ?? ""
            };

            _context.FinancialAccounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = account.Id }, account);
        }

        /// <summary>
        /// Atualiza uma conta financeira.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFinancialAccountDto model)
        {
            var companyId = await GetCurrentCompanyId();
            var account = await _context.FinancialAccounts.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (account == null) return NotFound();

            account.Name = model.Name;
            account.Active = model.Active;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Desativa uma conta financeira.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var companyId = await GetCurrentCompanyId();
            var account = await _context.FinancialAccounts.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

            if (account == null) return NotFound();

            account.Active = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}