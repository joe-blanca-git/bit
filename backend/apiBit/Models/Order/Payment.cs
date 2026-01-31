using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ExternalId { get; set; } = string.Empty; // ID da cobrança no ASAAS (pay_123456)
        
        public string Method { get; set; } = "CREDIT_CARD"; // CREDIT_CARD, PIX, BOLETO
        public string Status { get; set; } = "PENDING"; // PENDING, RECEIVED, OVERDUE

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidDate { get; set; }
        public DateTime DueDate { get; set; }

        // Dados visuais do Cartão (SEGURO PARA SALVAR)
        public string? CardBrand { get; set; } // Mastercard, Visa
        public string? CardLast4 { get; set; } // 1234
        
        public string? UrlInvoice { get; set; } // Link da fatura/comprovante

        public Guid OrderId { get; set; }
        [JsonIgnore]
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}