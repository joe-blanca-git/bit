using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using apiBit.Models;

namespace apiBit.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Paid, Canceled, Failed

        // Quem comprou?
        public string UserId { get; set; } = string.Empty;
        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Para qual empresa?
        public Guid CompanyId { get; set; }
        [JsonIgnore]
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        public List<OrderItem> Items { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
    }
}