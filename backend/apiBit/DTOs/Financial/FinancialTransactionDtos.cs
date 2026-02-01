using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class CreateFinancialTransactionDto
    {
        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        // 1 = Receita, 2 = Despesa
        [Required]
        public int Type { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime DocumentDate { get; set; } // Data da compra/venda

        // === CAMPOS PARA GERAÇÃO DE PARCELAS ===
        [Required]
        [Range(1, 999)]
        public int InstallmentsCount { get; set; } = 1; // Padrão 1 (À vista)

        [Required]
        public DateTime FirstDueDate { get; set; } // Vencimento da 1ª parcela

        // === VÍNCULOS (OPCIONAIS) ===
        public Guid? CategoryId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? OriginId { get; set; }
        public Guid? PersonId { get; set; }
    }

    // DTO simples para responder o que foi criado
    public class FinancialTransactionResponseDto
    {
        public Guid Id { get; set; }
        
        // Dados Principais
        public string Description { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Type { get; set; } // 1 = Receita, 2 = Despesa
        public DateTime DocumentDate { get; set; }
        
        // Dados Relacionados (ID e Nome para facilitar o front)
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public Guid? AccountId { get; set; }
        public string? AccountName { get; set; }

        public Guid? PersonId { get; set; }
        public string? PersonName { get; set; }

        public Guid? OriginId { get; set; }
        public string? OriginDescription { get; set; }

        // Dados de Auditoria
        public DateTime CreatedAt { get; set; }

        // Parcelas
        public int InstallmentsCount { get; set; }
        public List<FinancialInstallmentDto> Installments { get; set; } = new();
    }

    public class FinancialInstallmentDto
    {
        public int Number { get; set; }
        public decimal Value { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
    }
    
}