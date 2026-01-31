using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class CompanyDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CNPJ é obrigatório.")]
        public string Document { get; set; } = string.Empty;

        [Required]
        [RegularExpression("[SVT]", ErrorMessage = "A atividade deve ser 'S' (Serviços), 'V' (Vendas) ou 'T' (Todos).")]
        public string Activity { get; set; } = "S";

        public List<AddressDto> Addresses { get; set; } = new();
    }
}