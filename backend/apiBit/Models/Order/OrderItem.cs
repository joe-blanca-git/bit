using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace apiBit.Models
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty; // Ex: "Assinatura Plano Basic"

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Preço congelado no momento da compra
        
        public int Quantity { get; set; } = 1;

        // Vínculos
        public Guid OrderId { get; set; }
        [JsonIgnore]
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        public Guid? PlanId { get; set; } // Opcional, caso vendamos algo que não seja plano
        [JsonIgnore]
        [ForeignKey("PlanId")]
        public Plan? Plan { get; set; }
    }
}