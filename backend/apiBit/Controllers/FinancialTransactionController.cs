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
    public class FinancialTransactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FinancialTransactionController(AppDbContext context)
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
        /// Cria um novo lançamento financeiro (Receita ou Despesa) e gera as parcelas.
        /// </summary>
        /// <remarks>
        /// Este endpoint realiza o cálculo automático das parcelas ("split" do valor).
        /// 
        /// **Regras de Cálculo:**
        /// - O valor total é dividido pelo número de parcelas.
        /// - Eventuais diferenças de centavos (dízimas) são somadas à **última parcela**.
        /// - As datas de vencimento são incrementadas mensalmente a partir da `firstDueDate`.
        /// 
        /// **Exemplo de Requisição (Notebook de R$ 3.500,00 em 3x):**
        /// 
        ///     POST /api/FinancialTransaction
        ///     {
        ///        "description": "Compra Notebook Dell",
        ///        "type": 2,                           // 1 = Receita, 2 = Despesa
        ///        "totalAmount": 3500.00,
        ///        "documentDate": "2026-01-31",        // Data da nota/documento
        ///        "installmentsCount": 3,              // Quantidade de parcelas
        ///        "firstDueDate": "2026-02-15",        // Vencimento da 1ª parcela
        ///        "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", 
        ///        "accountId": null,                   // Pode ser nulo se não souber a conta ainda
        ///        "originId": null,
        ///        "personId": null
        ///     }
        /// 
        /// </remarks>
        /// <param name="model">Dados da transação e regras de parcelamento</param>
        /// <returns>A transação criada e a lista de parcelas geradas</returns>
        /// <response code="201">Transação criada com sucesso.</response>
        /// <response code="400">Dados inválidos (ex: Valor negativo, Empresa não encontrada).</response>
        /// <response code="401">Usuário não autenticado.</response>
        [HttpPost]
        [ProducesResponseType(typeof(FinancialTransactionResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateFinancialTransactionDto model)
        {
            // Validações Manuais Extras (além do DataAnnotations do DTO)
            if (model.TotalAmount <= 0)
            {
                return BadRequest(new ErrorResponseDto { Message = "O valor total deve ser maior que zero." });
            }

            var companyId = await GetCurrentCompanyId();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == Guid.Empty) 
            {
                return BadRequest(new ErrorResponseDto { Message = "Empresa não encontrada para o usuário logado." });
            }

            // 1. Criar o Cabeçalho (Transaction)
            var transaction = new FinancialTransaction
            {
                CompanyId = companyId,
                Description = model.Description,
                Type = model.Type,
                TotalAmount = model.TotalAmount,
                DocumentDate = model.DocumentDate,
                
                CategoryId = model.CategoryId,
                AccountId = model.AccountId,
                OriginId = model.OriginId,
                PersonId = model.PersonId,

                CreatedBy = userId ?? "System",
                CreatedAt = DateTime.Now
            };

            // 2. Lógica de Parcelamento (Cálculo de Centavos)
            decimal baseValue = Math.Floor((model.TotalAmount / model.InstallmentsCount) * 100) / 100;
            decimal remainder = model.TotalAmount - (baseValue * model.InstallmentsCount);

            for (int i = 1; i <= model.InstallmentsCount; i++)
            {
                var installmentValue = baseValue;

                // Se for a última parcela, soma o resto
                if (i == model.InstallmentsCount)
                {
                    installmentValue += remainder;
                }

                var installment = new FinancialInstallment
                {
                    InstallmentNumber = i,
                    Value = installmentValue,
                    // Adiciona meses: Parcela 1 = Data Inicial, Parcela 2 = +1 mês, etc.
                    DueDate = model.FirstDueDate.AddMonths(i - 1), 
                    Status = "Open"
                };

                transaction.Installments.Add(installment);
            }

            try 
            {
                _context.FinancialTransactions.Add(transaction);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Tratamento de erro genérico de banco
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Erro ao salvar transação.", 
                    Errors = new[] { ex.Message } 
                });
            }

            // 3. Montar Resposta
            // 4. Montar Resposta (Atualizado)
            var response = new FinancialTransactionResponseDto
            {
                Id = transaction.Id,
                Description = transaction.Description,
                TotalAmount = transaction.TotalAmount,
                Type = transaction.Type,
                DocumentDate = transaction.DocumentDate,
                CreatedAt = transaction.CreatedAt,

                // Como acabamos de criar, não temos os objetos carregados (Category, Person),
                // apenas os IDs que vieram do 'model'. 
                // Se quiser os nomes, teria que buscar no banco, mas retornar os IDs já ajuda.
                CategoryId = transaction.CategoryId,
                AccountId = transaction.AccountId,
                PersonId = transaction.PersonId,
                OriginId = transaction.OriginId,
                
                InstallmentsCount = transaction.Installments.Count,
                Installments = transaction.Installments.Select(i => new FinancialInstallmentDto
                {
                    Number = i.InstallmentNumber,
                    Value = i.Value,
                    DueDate = i.DueDate,
                    Status = i.Status
                }).ToList()
            };

            return CreatedAtAction(nameof(Create), new { id = transaction.Id }, response);

            return CreatedAtAction(nameof(Create), new { id = transaction.Id }, response);
        }

        /// <summary>
        /// Lista as transações financeiras com filtros avançados.
        /// </summary>
        /// <remarks>
        /// Permite filtrar por texto (Q), datas, categoria, pessoa, conta, etc.
        /// O filtro 'Q' busca no nome da pessoa, descrição, categoria ou nome da conta.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(List<FinancialTransactionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] FinancialTransactionFilterDto filter)
        {
            var companyId = await GetCurrentCompanyId();

            var query = _context.FinancialTransactions
                                .AsNoTracking()
                                .Include(t => t.Person)
                                .Include(t => t.Category)
                                .Include(t => t.Account)
                                .Include(t => t.Origin)
                                .Include(t => t.Installments)
                                .Where(t => t.CompanyId == companyId);

            // === FILTROS (Mantive igual ao anterior) ===
            if (filter.StartDate.HasValue)
                query = query.Where(t => t.DocumentDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
            {
                var endOfDay = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.DocumentDate <= endOfDay);
            }

            if (filter.PersonId.HasValue) query = query.Where(t => t.PersonId == filter.PersonId);
            if (filter.CategoryId.HasValue) query = query.Where(t => t.CategoryId == filter.CategoryId);
            if (filter.OriginId.HasValue) query = query.Where(t => t.OriginId == filter.OriginId);
            if (filter.AccountId.HasValue) query = query.Where(t => t.AccountId == filter.AccountId);
            if (filter.Type.HasValue) query = query.Where(t => t.Type == filter.Type);

            if (!string.IsNullOrEmpty(filter.Q))
            {
                string term = filter.Q.ToLower();
                query = query.Where(t => 
                    t.Description.ToLower().Contains(term) ||
                    (t.Person != null && t.Person.Name.ToLower().Contains(term)) ||
                    (t.Category != null && t.Category.Name.ToLower().Contains(term)) ||
                    (t.Account != null && t.Account.Name.ToLower().Contains(term))
                );
            }

            // === ORDENAÇÃO E PAGINAÇÃO ===
            query = query.OrderByDescending(t => t.DocumentDate);

            var totalItems = await query.CountAsync();
            Response.Headers.Add("X-Total-Count", totalItems.ToString());
            
            // === PROJEÇÃO (Select Completo) ===
            var transactions = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new FinancialTransactionResponseDto
                {
                    Id = t.Id,
                    Description = t.Description,
                    TotalAmount = t.TotalAmount,
                    Type = t.Type,
                    DocumentDate = t.DocumentDate,
                    CreatedAt = t.CreatedAt,
                    
                    // Mapeamento dos Relacionamentos
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category != null ? t.Category.Name : null,
                    
                    AccountId = t.AccountId,
                    AccountName = t.Account != null ? t.Account.Name : null,
                    
                    PersonId = t.PersonId,
                    PersonName = t.Person != null ? t.Person.Name : null,
                    
                    OriginId = t.OriginId,
                    OriginDescription = t.Origin != null ? t.Origin.Description : null,

                    InstallmentsCount = t.Installments.Count,
                    Installments = t.Installments.Select(i => new FinancialInstallmentDto
                    {
                        Number = i.InstallmentNumber,
                        Value = i.Value,
                        DueDate = i.DueDate,
                        Status = i.Status
                    }).OrderBy(i => i.Number).ToList()
                })
                .ToListAsync();

            return Ok(transactions);
        }

        /// <summary>
        /// Atualiza uma transação financeira existente e redefini suas parcelas.
        /// </summary>
        /// <remarks>
        /// **Regras de validação:**
        /// 1. A soma dos valores das parcelas enviadas DEVE ser exatamente igual ao `TotalAmount`.
        /// 2. Não é permitido alterar valores/parcelas se a transação já possuir parcelas pagas (Status diferente de 'Open').
        /// 3. O método substitui todas as parcelas antigas pelas novas enviadas no array.
        /// </remarks>
        /// <param name="id">ID da transação</param>
        /// <param name="model">Dados atualizados</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFinancialTransactionDto model)
        {
            // 1. Validação Matemática (Frontend deve mandar certo, mas Backend não confia)
            decimal sumInstallments = model.Installments.Sum(i => i.Value);
            
            // Arredondamento para evitar erro de dízima (ex: 99.9999 != 100.00)
            if (Math.Round(sumInstallments, 2) != Math.Round(model.TotalAmount, 2))
            {
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Divergência de Valores", 
                    Errors = new[] { $"A soma das parcelas ({sumInstallments:C}) não bate com o valor total ({model.TotalAmount:C})." } 
                });
            }

            var companyId = await GetCurrentCompanyId();

            // 2. Busca Transação + Parcelas atuais
            var transaction = await _context.FinancialTransactions
                                            .Include(t => t.Installments)
                                            .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);

            if (transaction == null) return NotFound(new { message = "Transação não encontrada." });

            // 3. Trava de Segurança: Verifica se já existe pagamento
            bool hasPayments = transaction.Installments.Any(i => i.Status != "Open");
            if (hasPayments)
            {
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Operação Bloqueada", 
                    Errors = new[] { "Não é possível alterar valores ou parcelas de uma transação que já possui pagamentos ou baixas parciais. Exclua os pagamentos antes de editar." } 
                });
            }

            // 4. Atualiza Cabeçalho
            transaction.Description = model.Description;
            transaction.TotalAmount = model.TotalAmount;
            transaction.DocumentDate = model.DocumentDate;
            transaction.CategoryId = model.CategoryId;
            transaction.AccountId = model.AccountId;
            transaction.OriginId = model.OriginId;
            transaction.PersonId = model.PersonId;
            transaction.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            transaction.UpdatedAt = DateTime.Now;

            // 5. Atualiza Parcelas (Estratégia: Limpar e Recriar)
            // Como validamos que nenhuma está paga, podemos remover sem medo.
            _context.FinancialInstallments.RemoveRange(transaction.Installments);
            
            // Adiciona as novas
            foreach (var item in model.Installments)
            {
                transaction.Installments.Add(new FinancialInstallment
                {
                    InstallmentNumber = item.InstallmentNumber,
                    Value = item.Value,
                    DueDate = item.DueDate,
                    Status = "Open", // Como estamos recriando, volta a ser Open
                    TransactionId = transaction.Id
                });
            }

            try
            {
                await _context.SaveChangesAsync();
                return NoContent(); // 204 - Sucesso sem retorno
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Erro ao atualizar transação", 
                    Errors = new[] { ex.Message } 
                });
            }
        }

        /// <summary>
        /// Exclui uma transação e suas parcelas.
        /// </summary>
        /// <remarks>
        /// Só é permitido excluir se NÃO houver pagamentos vinculados.
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var companyId = await GetCurrentCompanyId();

            var transaction = await _context.FinancialTransactions
                                            .Include(t => t.Installments) // Necessário verificar status
                                            .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == companyId);

            if (transaction == null) return NotFound();

            // Valida Pagamentos
            if (transaction.Installments.Any(i => i.Status != "Open"))
            {
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Não é possível excluir", 
                    Errors = new[] { "Esta transação possui parcelas pagas. Estorne os pagamentos antes de excluir." } 
                });
            }

            _context.FinancialTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}