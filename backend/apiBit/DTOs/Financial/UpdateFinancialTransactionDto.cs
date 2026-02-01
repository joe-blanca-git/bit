using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class UpdateFinancialTransactionDto
    {
        // === CABEÇALHO ===
        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime DocumentDate { get; set; }

        // IDs vinculados (Opcionais)
        public Guid? CategoryId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? OriginId { get; set; }
        public Guid? PersonId { get; set; }

        // === PARCELAS (Obrigatório enviar a lista completa e redistribuída) ===
        [Required]
        public List<UpdateFinancialInstallmentDto> Installments { get; set; } = new();
    }

    public class UpdateFinancialInstallmentDto
    {
        [Required]
        public int InstallmentNumber { get; set; } // 1, 2, 3...

        [Required]
        public decimal Value { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}