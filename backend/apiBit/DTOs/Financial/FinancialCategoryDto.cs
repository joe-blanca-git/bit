using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    // Para Criar
    public class CreateFinancialCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 2, ErrorMessage = "Type must be 1 (Income) or 2 (Expense)")]
        public int Type { get; set; }
    }

    // Para Atualizar
    public class UpdateFinancialCategoryDto : CreateFinancialCategoryDto
    {
        [Required]
        public bool Active { get; set; }
    }

    // Para Retornar
    public class FinancialCategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; } // 1 = Receita, 2 = Despesa
        public bool Active { get; set; }
    }
}