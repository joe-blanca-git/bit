using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class FinancialAccount
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CompanyId { get; set; }
        [JsonIgnore]
        public Company? Company { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool Active { get; set; } = true; // A = true, I = false

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
    }
}