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
    public class FinancialPaymentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FinancialPaymentController(AppDbContext context)
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
        /// Realiza a baixa (pagamento ou recebimento) de uma parcela.
        /// </summary>
        /// <remarks>
        /// Este endpoint registra a movimentação financeira (Regime de Caixa) e atualiza o status da parcela.
        /// 
        /// **Cenários Possíveis:**
        /// 1. **Pagamento Total:** Valor pago == Valor da parcela (Status -> Paid).
        /// 2. **Pagamento Parcial:** Valor pago &lt; Valor da parcela e `IsFullySettled` = false (Status -> Partial).
        /// 3. **Pagamento com Desconto:** Valor pago &lt; Valor da parcela e `IsFullySettled` = true (Status -> Paid).
        /// 4. **Pagamento com Juros:** Valor pago &gt; Valor da parcela (Status -> Paid).
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(FinancialPaymentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Pay([FromBody] CreateFinancialPaymentDto model)
        {
            if (model.AmountPaid <= 0)
                return BadRequest(new ErrorResponseDto { Message = "O valor do pagamento deve ser maior que zero." });

            var companyId = await GetCurrentCompanyId();

            // 1. Busca a Parcela e seus pagamentos anteriores para validar
            var installment = await _context.FinancialInstallments
                                            .Include(i => i.Transaction)
                                            .Include(i => i.Transaction!.Payments) // Carrega pagamentos já feitos
                                            .FirstOrDefaultAsync(i => i.Id == model.InstallmentId);

            if (installment == null)
                return NotFound(new { message = "Parcela não encontrada." });

            // Valida se pertence à empresa
            if (installment.Transaction!.CompanyId != companyId)
                return NotFound(new { message = "Parcela não encontrada." });

            // Valida Conta Bancária
            var account = await _context.FinancialAccounts
                                        .FirstOrDefaultAsync(a => a.Id == model.AccountId && a.CompanyId == companyId);
            if (account == null)
                return BadRequest(new ErrorResponseDto { Message = "Conta financeira inválida." });

            // 2. Cria o registro do Pagamento
            var payment = new FinancialPayment
            {
                CompanyId = companyId,
                TransactionId = installment.TransactionId,
                InstallmentId = installment.Id,
                AmountPaid = model.AmountPaid,
                PaymentDate = model.PaymentDate,
                PaymentMethod = model.PaymentMethod,
                CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                CreatedAt = DateTime.Now
            };

            _context.FinancialPayments.Add(payment);

            // 3. Lógica de Atualização de Status (A Mágica)
            // Soma o que já foi pago antes + o atual
            decimal totalPaidSoFar = installment.Transaction.Payments
                                                .Where(p => p.InstallmentId == installment.Id)
                                                .Sum(p => p.AmountPaid) + model.AmountPaid;

            if (model.IsFullySettled || totalPaidSoFar >= installment.Value)
            {
                installment.Status = "Paid"; // Quitado (mesmo com desconto ou juros)
            }
            else
            {
                installment.Status = "Partial"; // Ainda falta pagar
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseDto { Message = "Erro ao processar pagamento", Errors = new[] { ex.Message } });
            }

            // 4. Retorno
            var response = new FinancialPaymentResponseDto
            {
                Id = payment.Id,
                InstallmentId = payment.InstallmentId.Value,
                AmountPaid = payment.AmountPaid,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                AccountName = account.Name
            };

            return CreatedAtAction(nameof(Pay), new { id = payment.Id }, response);
        }

        /// <summary>
        /// Estorna (exclui) um pagamento realizado.
        /// </summary>
        /// <remarks>
        /// Ao excluir um pagamento, o status da parcela será recalculado automaticamente 
        /// (podendo voltar para 'Open' ou 'Partial').
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RollbackPayment(Guid id)
        {
            var companyId = await GetCurrentCompanyId();

            // Busca o pagamento e a parcela vinculada
            var payment = await _context.FinancialPayments
                                        .Include(p => p.Installment)
                                        .ThenInclude(i => i!.Transaction)
                                        .ThenInclude(t => t!.Payments) // Precisamos de todos os pagamentos irmãos para recalcular
                                        .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

            if (payment == null) return NotFound(new { message = "Pagamento não encontrado." });

            var installment = payment.Installment;

            // 1. Remove o pagamento
            _context.FinancialPayments.Remove(payment);

            // 2. Recalcula o Status da Parcela (Rollback Logic)
            if (installment != null)
            {
                // Soma dos pagamentos RESTANTES (excluindo o atual que vai ser apagado)
                decimal remainingPaymentsSum = installment.Transaction!.Payments
                                                .Where(p => p.InstallmentId == installment.Id && p.Id != payment.Id)
                                                .Sum(p => p.AmountPaid);

                if (remainingPaymentsSum >= installment.Value)
                {
                    installment.Status = "Paid"; // Ainda continua pago por outros pagamentos
                }
                else if (remainingPaymentsSum > 0)
                {
                    installment.Status = "Partial"; // Voltou a ser parcial
                }
                else
                {
                    installment.Status = "Open"; // Não tem mais nenhum pagamento, voltou a ficar em aberto
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Quita (baixa) todas as parcelas pendentes de uma transação de uma só vez.
        /// </summary>
        /// <remarks>
        /// Útil para quando o cliente paga a dívida toda ou você quita uma compra parcelada antecipadamente.
        /// 
        /// **Lógica de Distribuição do Valor:**
        /// 1. O sistema identifica todas as parcelas que NÃO estão totalmente pagas ("Open" ou "Partial").
        /// 2. As parcelas iniciais recebem o pagamento do valor exato devedor.
        /// 3. A **última parcela** recebe todo o saldo restante do valor informado.
        ///    - Se o valor for menor que a dívida total, a diferença é considerada **Desconto**.
        ///    - Se o valor for maior, a diferença é considerada **Juros/Multa**.
        /// 4. Todas as parcelas são marcadas como "Paid".
        /// </remarks>
        [HttpPost("settle-transaction")]
        [ProducesResponseType(typeof(SettleTransactionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SettleTransaction([FromBody] SettleTransactionDto model)
        {
            if (model.TotalAmountPaid <= 0)
                return BadRequest(new ErrorResponseDto { Message = "O valor total pago deve ser maior que zero." });

            var companyId = await GetCurrentCompanyId();

            // 1. Busca a transação e as parcelas ABERTAS
            var transaction = await _context.FinancialTransactions
                                            .Include(t => t.Installments)
                                            .ThenInclude(i => i.Payments) // Precisamos ver o que já foi pago
                                            .FirstOrDefaultAsync(t => t.Id == model.TransactionId && t.CompanyId == companyId);

            if (transaction == null)
                return NotFound(new { message = "Transação não encontrada." });

            // Filtra só o que precisa ser pago (Open ou Partial) e ordena por data/número
            var openInstallments = transaction.Installments
                                              .Where(i => i.Status != "Paid")
                                              .OrderBy(i => i.InstallmentNumber)
                                              .ToList();

            if (!openInstallments.Any())
                return BadRequest(new ErrorResponseDto { Message = "Esta transação já está totalmente quitada." });

            // Valida conta
            var accountExists = await _context.FinancialAccounts
                                              .AnyAsync(a => a.Id == model.AccountId && a.CompanyId == companyId);
            if (!accountExists)
                return BadRequest(new ErrorResponseDto { Message = "Conta financeira inválida." });

            // 2. Variável de controle do dinheiro disponível para distribuir
            decimal remainingMoney = model.TotalAmountPaid;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            int processedCount = 0;

            // 3. Loop de Distribuição
            for (int i = 0; i < openInstallments.Count; i++)
            {
                var installment = openInstallments[i];
                bool isLastItem = (i == openInstallments.Count - 1);

                // Calcula quanto falta pagar desta parcela específica
                decimal alreadyPaidOnThis = installment.Payments.Sum(p => p.AmountPaid);
                decimal debtBalance = installment.Value - alreadyPaidOnThis;

                decimal paymentAmount = 0;

                if (isLastItem)
                {
                    // Se for a última, joga TUDO o que sobrou aqui.
                    // Isso absorve centavos, descontos ou juros automaticamente.
                    paymentAmount = remainingMoney;
                }
                else
                {
                    // Se não for a última, tenta pagar o valor exato da dívida dela
                    // Mas se o dinheiro total informado for menor que a dívida (desconto global absurdo),
                    // usamos o que tem.
                    paymentAmount = debtBalance;
                    
                    // Proteção: Se o dinheiro acabou antes da última parcela (caso raro de desconto muito agressivo)
                    if (remainingMoney < paymentAmount) 
                        paymentAmount = remainingMoney;
                }

                // Cria o pagamento se houver valor (ou se for a última, mesmo que seja 0 para quitar com 100% desconto)
                if (paymentAmount > 0 || (isLastItem && paymentAmount == 0))
                {
                    var payment = new FinancialPayment
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = companyId,
                        TransactionId = transaction.Id,
                        InstallmentId = installment.Id,
                        AmountPaid = paymentAmount,
                        PaymentDate = model.PaymentDate,
                        PaymentMethod = model.PaymentMethod,
                        AccountId = model.AccountId,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now
                    };

                    _context.FinancialPayments.Add(payment);
                    remainingMoney -= paymentAmount; // Desconta da carteira virtual
                }

                // Força o status para PAGO (pois é uma quitação total solicitada pelo usuário)
                installment.Status = "Paid";
                processedCount++;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseDto 
                { 
                    Message = "Erro ao processar quitação em lote", 
                    Errors = new[] { ex.Message } 
                });
            }

            return Ok(new SettleTransactionResponseDto
            {
                Message = "Transação quitada com sucesso!",
                InstallmentsSettledCount = processedCount,
                TotalAmountPaid = model.TotalAmountPaid
            });
        }
    }
}