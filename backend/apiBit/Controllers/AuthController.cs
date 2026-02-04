using apiBit.Data;
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;    

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public AuthController(IAuthService authService, UserManager<User> userManager, AppDbContext context)
        {
            _authService = authService;
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Registra um novo usuário na plataforma Bit.
        /// </summary>
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
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)] 
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)] 
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            // 1. Valida credenciais e gera token usando seu Service original
            var result = await _authService.Login(model);

            if (result == null)
            {
                return Unauthorized(new { message = "Email ou senha incorretos." });
            }

            // 2. Busca dados extras para montar o JSON completo
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // Roles como lista simples
            var roles = await _userManager.GetRolesAsync(user);

            // Dados da Empresa
            var company = await _context.Companies
                                .Include(c => c.Plan)
                                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            
            // Dados do Perfil
            var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == user.Id);

            // 3. Retorna JSON formatado corretamente
            return Ok(new
            {
                token = result.Token, 
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = person != null ? person.Name : user.Name,
                    userName = user.UserName
                },
                roles = roles, 
                
                company = company == null ? null : new {
                    id = company.Id,
                    name = company.Name,
                    planId = company.PlanId,
                    planName = company.Plan?.Name,
                    status = company.SubscriptionStatus,
                    expiresAt = company.SubscriptionExpiresAt
                },

                menus = result.Menus
            });
        }

        /// <summary>
        /// Envia um e-mail de recuperação de senha.
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            await _authService.ForgotPassword(model.Email);
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
            // AQUI ESTAVA O ERRO: O NOME ERA 'LOGIN' E O CODIGO ERA DE LOGIN.
            // AGORA ESTÁ CORRIGIDO PARA RESETPASSWORD
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
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
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