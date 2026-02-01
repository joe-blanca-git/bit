using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class CreateFinancialPaymentDto
    {
        [Required]
        public Guid InstallmentId { get; set; }

        [Required]
        public Guid AccountId { get; set; } // De qual conta saiu/entrou o dinheiro?

        [Required]
        public decimal AmountPaid { get; set; } // Valor Efetivamente Pago

        [Required]
        public DateTime PaymentDate { get; set; } // Data do pagamento real

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Pix, Dinheiro, Boleto...

        /// <summary>
        /// Indica se este pagamento quita a parcela, mesmo que o valor seja menor (Desconto).
        /// Se false e o valor for menor, a parcela fica como 'Partial'.
        /// </summary>
        public bool IsFullySettled { get; set; } = true; 
    }

    public class FinancialPaymentResponseDto
    {
        public Guid Id { get; set; }
        public Guid InstallmentId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
    }
}