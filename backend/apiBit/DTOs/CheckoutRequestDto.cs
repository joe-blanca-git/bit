using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs.Asaas
{
    public class CheckoutRequestDto
    {
        [Required]
        public Guid PlanId { get; set; } // O que ele está comprando

        [Required]
        public CreditCardDto CreditCard { get; set; } = new();

        [Required]
        public CreditCardHolderInfoDto HolderInfo { get; set; } = new();
    }
}