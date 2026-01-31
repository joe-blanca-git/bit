using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs.Asaas
{
    public class CheckoutRequestDto
    {
        [Required]
        public Guid PlanId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "CREDIT_CARD"; 

        public CreditCardDto? CreditCard { get; set; } 

        [Required]
        public CreditCardHolderInfoDto HolderInfo { get; set; } = new();
    }
}