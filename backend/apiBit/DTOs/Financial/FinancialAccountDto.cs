using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class CreateFinancialAccountDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateFinancialAccountDto : CreateFinancialAccountDto
    {
        [Required]
        public bool Active { get; set; }
    }

    public class FinancialAccountResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}