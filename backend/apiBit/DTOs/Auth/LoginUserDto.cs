using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class LoginUserDto
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        public string Password { get; set; } = string.Empty;
    }
}