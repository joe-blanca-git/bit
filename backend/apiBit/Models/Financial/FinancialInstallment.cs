using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class FinancialInstallment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TransactionId { get; set; }
        [JsonIgnore]
        public FinancialTransaction? Transaction { get; set; }

        public int InstallmentNumber { get; set; } // Parcela 1, 2, 3...

        public DateTime DueDate { get; set; } // Data de Vencimento

        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        // Status da Parcela: "Open", "Paid", "Partial", "Overdue"
        public string Status { get; set; } = "Open"; 
    }
}