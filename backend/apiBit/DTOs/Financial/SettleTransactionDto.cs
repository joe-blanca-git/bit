using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class SettleTransactionDto
    {
        [Required]
        public Guid TransactionId { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public decimal TotalAmountPaid { get; set; } 

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class SettleTransactionResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int InstallmentsSettledCount { get; set; }
        public decimal TotalAmountPaid { get; set; }
    }
}