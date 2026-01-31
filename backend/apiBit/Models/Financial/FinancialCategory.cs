using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class FinancialCategory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CompanyId { get; set; }
        [JsonIgnore]
        public Company? Company { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // 1 = Income (Receita), 2 = Expense (Despesa)
        public int Type { get; set; } 

        public bool Active { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
    }
}