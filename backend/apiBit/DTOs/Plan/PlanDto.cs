using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class PlanDto
    {
        [Required(ErrorMessage = "O nome do plano é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999, ErrorMessage = "O valor deve ser maior ou igual a zero.")]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }
    }
}