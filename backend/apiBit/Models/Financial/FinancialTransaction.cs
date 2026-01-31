using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class FinancialTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CompanyId { get; set; }
        [JsonIgnore]
        public Company? Company { get; set; }

        // Cliente ou Fornecedor (Pode ser nulo)
        public Guid? PersonId { get; set; }
        [JsonIgnore]
        public Person? Person { get; set; }

        // Conta prevista (pode mudar no pagamento, mas é bom ter o padrão)
        public Guid? AccountId { get; set; }
        [JsonIgnore]
        public FinancialAccount? Account { get; set; }

        public Guid? CategoryId { get; set; }
        [JsonIgnore]
        public FinancialCategory? Category { get; set; }

        public Guid? OriginId { get; set; }
        [JsonIgnore]
        public FinancialOrigin? Origin { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        // 1 = Income, 2 = Expense
        public int Type { get; set; }

        // Valor Total da Transação
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime DocumentDate { get; set; } // Data de Emissão/Competência

        // Auditoria
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Relacionamento com Parcelas e Pagamentos
        public List<FinancialInstallment> Installments { get; set; } = new();
        public List<FinancialPayment> Payments { get; set; } = new();
    }
}