using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class RegisterFullUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public PersonDto ProfileData { get; set; }

        /// <summary>
        /// Tipo de usuário: "CLIENT", "SUPPLIER" ou "CARRIER"
        /// </summary>
        [Required]
        [RegularExpression("CLIENT|SUPPLIER|CARRIER", ErrorMessage = "O tipo deve ser CLIENT, SUPPLIER ou CARRIER.")]
        public string UserType { get; set; } = string.Empty;
    }
}