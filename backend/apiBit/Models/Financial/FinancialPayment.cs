using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class FinancialPayment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TransactionId { get; set; }
        [JsonIgnore]
        public FinancialTransaction? Transaction { get; set; }

        // Pode ser nulo, pois posso pagar o total da transação sem especificar parcela
        // Ou pagar várias parcelas (aí teríamos que ter outra lógica, mas 1 pra 1 é mais simples agora)
        public Guid? InstallmentId { get; set; }
        [JsonIgnore]
        public FinancialInstallment? Installment { get; set; }

        public Guid CompanyId { get; set; }

        public Guid AccountId { get; set; }
        [JsonIgnore]
        public FinancialAccount? Account { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        // Dinheiro, Pix, Cartão, Boleto, Transferência
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        
    }
}