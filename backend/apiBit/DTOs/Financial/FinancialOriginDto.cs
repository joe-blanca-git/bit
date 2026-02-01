using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class CreateFinancialOriginDto
    {
        [Required]
        [MaxLength(100)]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateFinancialOriginDto : CreateFinancialOriginDto
    {
        [Required]
        public bool Active { get; set; }
    }

    public class FinancialOriginResponseDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}