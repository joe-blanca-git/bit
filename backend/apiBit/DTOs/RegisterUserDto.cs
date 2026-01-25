using System.ComponentModel.DataAnnotations;

namespace apiBit.DTOs
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de E-mail invalido.")]
        public string Email { get; set;}

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        [MinLength(8, ErrorMessage = "A Senha deve ter no mínimo 8 caracteres.")]
        public string Password {get; set;}

        [Compare("Password", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmPassword {get; set;}

    }
}