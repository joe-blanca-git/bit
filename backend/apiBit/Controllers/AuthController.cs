using apiBit.DTOs;
using apiBit.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims;      

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registra um novo usuário na plataforma Bit.
        /// </summary>
        /// <remarks>
        /// Cria o usuário no sistema de autenticação. Após isso, o usuário deve completar o perfil em POST /Person/profile.
        /// </remarks>
        /// <param name="model">Dados de registro (Email e Senha)</param>
        /// <response code="200">Usuário criado com sucesso.</response>
        /// <response code="400">Dados inválidos ou e-mail já existente.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            var result = await _authService.RegisterUser(model);

            if (result.Succeeded)
            {
                return Ok(new { message = "Usuário registrado com sucesso!" });
            }

            // Converte os erros do Identity para o nosso padrão DTO
            var errorResponse = new ErrorResponseDto
            {
                Message = "Erro ao registrar usuário",
                Errors = result.Errors.Select(e => e.Description).ToArray()
            };

            return BadRequest(errorResponse);
        }

        /// <summary>
        /// Realiza o login e gera o Token JWT.
        /// </summary>
        /// <remarks>
        /// Retorna o Token de acesso e os dados básicos do usuário.
        /// </remarks>
        /// <param name="model">Credenciais (Email e Senha)</param>
        /// <returns>Token JWT e dados do usuário</returns>
        /// <response code="200">Login realizado com sucesso.</response>
        /// <response code="400">Dados de entrada inválidos.</response>
        /// <response code="401">Email ou senha incorretos.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)] // Supondo que você tenha esse DTO ou retorna objeto anônimo
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)] // <--- AQUI A MÁGICA PARA LIMPAR O SWAGGER
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            var result = await _authService.Login(model);

            if (result == null)
            {
                return Unauthorized(new { message = "Email ou senha incorretos." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Envia um e-mail de recuperação de senha.
        /// </summary>
        /// <remarks>
        /// Se o e-mail existir na base, o usuário receberá um link com o token.
        /// </remarks>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            await _authService.ForgotPassword(model.Email);
            // Sempre retornamos OK por segurança, para não revelar se o e-mail existe ou não
            return Ok(new { message = "Se o e-mail estiver cadastrado, você receberá um link de recuperação." });
        }

        /// <summary>
        /// Redefine a senha usando o token recebido por e-mail.
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await _authService.ResetPassword(model);

            if (result.Succeeded)
            {
                return Ok(new { message = "Senha alterada com sucesso!" });
            }

            return BadRequest(new ErrorResponseDto 
            { 
                Message = "Erro ao redefinir senha", 
                Errors = result.Errors.Select(e => e.Description).ToArray() 
            });
        }

        /// <summary>
        /// Altera a senha do usuário logado.
        /// </summary>
        /// <remarks>
        /// Requer a senha atual para validação.
        /// </remarks>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            // Pega o e-mail de dentro do token
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            
            if (email == null) return Unauthorized();

            var result = await _authService.ChangePassword(email, model);

            if (result.Succeeded)
            {
                return Ok(new { message = "Senha alterada com sucesso!" });
            }

            return BadRequest(new ErrorResponseDto 
            { 
                Message = "Erro ao alterar senha", 
                Errors = result.Errors.Select(e => e.Description).ToArray() 
            });
        }
    }
}