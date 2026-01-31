using apiBit.Data;
using apiBit.DTOs.Asaas;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            // Iniciamos uma transação: Se der erro no meio, desfaz tudo no banco
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Identificar o Usuário Logado
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null) return Unauthorized(new { message = "Usuário não identificado." });

                // 2. Buscar a Empresa deste Usuário
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.UserId == userId); 

                if (company == null)
                {
                    return BadRequest(new { message = "Nenhuma empresa vinculada a este usuário encontrada." });
                }

                // 3. Validar o Plano
                var plan = await _context.Plans.FindAsync(model.PlanId);
                if (plan == null) return BadRequest(new { message = "Plano não encontrado." });

                // 4. Criar a ORDEM DE COMPRA (Inicialmente Pendente)
                var order = new Order
                {
                    UserId = userId,
                    CompanyId = company.Id,
                    TotalAmount = plan.Price,
                    Status = "Pending", // Status inicial
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
                await _context.SaveChangesAsync(); // Salva para gerar o ID do Pedido

                // 5. Integração ASAAS: Criar ou Buscar Cliente
                var asaasCustomerId = await _asaasService.CreateCustomer(user, model.HolderInfo);

                // Variáveis para preencher dependendo do método
                string asaasPaymentId = "";
                string asaasStatus = "PENDING";
                string pixPayload = "";
                string pixImage = "";

                // === DECISÃO: PIX OU CARTÃO ===
                if (model.PaymentMethod == "PIX")
                {
                    // A. Cria cobrança PIX
                    asaasPaymentId = await _asaasService.CreatePixCharge(asaasCustomerId, plan.Price, order.Id.ToString());
                    
                    // B. Busca o QR Code para exibir no front
                    var qrCodeData = await _asaasService.GetPixQrCode(asaasPaymentId);
                    pixPayload = qrCodeData.payload;     // Código copia e cola
                    pixImage = qrCodeData.encodedImage;  // Imagem Base64
                }
                else // Assume CREDIT_CARD
                {
                    if (model.CreditCard == null) 
                        return BadRequest(new { message = "Dados do cartão são obrigatórios." });

                    // A. Cria cobrança no Cartão e JÁ PEGA O STATUS (Aprovado ou nao)
                    var result = await _asaasService.CreatePayment(
                        asaasCustomerId, 
                        plan.Price, 
                        model.CreditCard, 
                        model.HolderInfo,
                        order.Id.ToString()
                    );

                    asaasPaymentId = result.id;
                    asaasStatus = result.status; // CONFIRMED, RECEIVED, PENDING, REJECTED...
                }

                // 6. Salvar o registro do Pagamento (Payment) no Banco
                var payment = new Payment
                {
                    OrderId = order.Id,
                    ExternalId = asaasPaymentId,
                    Method = model.PaymentMethod, 
                    Status = asaasStatus, 
                    DueDate = DateTime.Now
                };

                // Se for cartão, salvamos o final dele para exibir depois
                if (model.PaymentMethod == "CREDIT_CARD" && model.CreditCard != null)
                {
                    payment.CardBrand = "Credit Card"; 
                    payment.CardLast4 = model.CreditCard.Number.Length > 4 
                        ? model.CreditCard.Number.Substring(model.CreditCard.Number.Length - 4) 
                        : "****";
                }

                _context.Payments.Add(payment);
                
                // 7. Lógica de Liberação Imediata (Cartão Aprovado na hora)
                if (asaasStatus == "CONFIRMED" || asaasStatus == "RECEIVED")
                {
                    order.Status = "Completed";
                    
                    // === ATIVAÇÃO DA EMPRESA ===
                    // O pagamento passou, então atualizamos a empresa com o plano e validade
                    await ActivateCompanySubscription(company.Id, plan.Id);
                }
                else
                {
                    // Se for PIX ou Cartão em Análise
                    order.Status = "Processing"; 
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 8. Retorno para o Front-end
                return Ok(new { 
                    message = "Pagamento processado!", 
                    orderId = order.Id, 
                    status = order.Status, // Se vier "Completed", o front redireciona.
                    asaasId = asaasPaymentId,
                    paymentMethod = model.PaymentMethod,
                    pixPayload = pixPayload, 
                    pixImage = pixImage      
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"ERRO CHECKOUT: {ex.Message}");
                return BadRequest(new { error = "Falha ao processar pagamento", details = ex.Message });
            }
        }

        // Endpoint para o Front ficar consultando (Polling)
        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetStatus(Guid orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Select(o => new { o.Status, o.Id }) 
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            return Ok(order);
        }

        // === MÉTODO AUXILIAR PARA ATIVAR O PLANO NA EMPRESA ===
        private async Task ActivateCompanySubscription(Guid companyId, Guid planId)
        {
            // Busca a empresa novamente (ou reutiliza se o contexto já estiver rastreando)
            var company = await _context.Companies.FindAsync(companyId);
            
            if (company != null)
            {
                company.PlanId = planId;
                company.SubscriptionStatus = "Active";
                
                // Adiciona 30 dias a partir de hoje (Lógica Mensal Simples)
                company.SubscriptionExpiresAt = DateTime.UtcNow.AddDays(30);

                _context.Companies.Update(company);
                await _context.SaveChangesAsync();
            }
        }
    }
}