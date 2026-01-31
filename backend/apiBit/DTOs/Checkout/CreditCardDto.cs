using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs.Asaas
{
    public class CreditCardDto
    {
        [Required] public string HolderName { get; set; } = string.Empty; // Nome impresso
        [Required] public string Number { get; set; } = string.Empty; // Número do cartão
        [Required] public string ExpiryMonth { get; set; } = string.Empty; // Mês (ex: 06)
        [Required] public string ExpiryYear { get; set; } = string.Empty; // Ano (ex: 2028)
        [Required] public string Ccv { get; set; } = string.Empty; // Código segurança
    }

    public class CreditCardHolderInfoDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string CpfCnpj { get; set; } = string.Empty;
        [Required] public string PostalCode { get; set; } = string.Empty;
        [Required] public string AddressNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}