using apiBit.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class Company
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)] // CNPJ
        public string Document { get; set; } = string.Empty;

        // S = Serviços, V = Vendas, T = Todos (Ambos)
        [Required]
        [MaxLength(1)]
        public string Activity { get; set; } = "S"; 

        // Dono da empresa
        [Required]
        public string UserId { get; set; } = string.Empty;

        public string SubscriptionStatus { get; set; } = "Inactive"; // Active, Inactive, Overdue
        
        public DateTime? SubscriptionExpiresAt { get; set; }
        
        [JsonIgnore]
        public User? User { get; set; }

        // Relacionamento com endereços da empresa
        public List<CompanyAddress> Addresses { get; set; } = new();

        public Guid? PlanId { get; set; }
        [JsonIgnore]
        public Plan? Plan { get; set; }
    }
}