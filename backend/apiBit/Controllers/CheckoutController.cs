using apiBit.DTOs.Asaas;
using apiBit.Interfaces;
using apiBit.Models;
using apiBit.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necessário para o FirstOrDefaultAsync
using System.Security.Claims;

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAsaasService _asaasService;
        private readonly UserManager<User> _userManager;

        public CheckoutController(AppDbContext context, IAsaasService asaasService, UserManager<User> userManager)
        {
            _context = context;
            _asaasService = asaasService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Identificar o Usuário Logado
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null) return Unauthorized(new { message = "Usuário não identificado." });

                var company = await _context.Companies
                                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (company == null)
                {
                    return BadRequest(new { message = "Nenhuma empresa vinculada a este usuário encontrada." });
                }

                // 3. Validar o Plano
                var plan = await _context.Plans.FindAsync(model.PlanId);
                if (plan == null) return BadRequest(new { message = "Plano não encontrado." });

                // 4. Criar a ORDEM DE COMPRA (Pendente)
                var order = new Order
                {
                    UserId = userId,
                    CompanyId = company.Id, // Agora usamos o ID real da empresa dele
                    TotalAmount = plan.Price,
                    Status = "Pending",
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Description = $"Assinatura {plan.Name}",
                            UnitPrice = plan.Price,
                            PlanId = plan.Id
                        }
                    }
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); 

                // 5. Integração ASAAS: Criar ou Buscar Cliente
                var asaasCustomerId = await _asaasService.CreateCustomer(user, model.HolderInfo);

                // 6. Integração ASAAS: Efetuar Cobrança
                var asaasPaymentId = await _asaasService.CreatePayment(
                    asaasCustomerId, 
                    plan.Price, 
                    model.CreditCard, 
                    model.HolderInfo,
                    order.Id.ToString()
                );

                // 7. Sucesso! Salvar o Pagamento Localmente
                var payment = new Payment
                {
                    OrderId = order.Id,
                    ExternalId = asaasPaymentId,
                    Method = "CREDIT_CARD",
                    Status = "PENDING", 
                    CardBrand = "Credit Card", // Podemos melhorar isso se o Asaas retornar a bandeira
                    CardLast4 = model.CreditCard.Number.Length > 4 
                        ? model.CreditCard.Number.Substring(model.CreditCard.Number.Length - 4) 
                        : "****",
                    DueDate = DateTime.Now
                };

                _context.Payments.Add(payment);
                
                // Atualiza o pedido para processando
                order.Status = "Processing";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { 
                    message = "Pagamento enviado com sucesso!", 
                    orderId = order.Id, 
                    asaasId = asaasPaymentId,
                    status = "PENDING"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Logar o erro real no console para você ver o que houve
                Console.WriteLine($"ERRO CHECKOUT: {ex.Message}");
                return BadRequest(new { error = "Falha ao processar pagamento", details = ex.Message });
            }
        }
    }
}