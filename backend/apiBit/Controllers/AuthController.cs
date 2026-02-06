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
        private readonly IPersonService _personService;
        private readonly IEmailService _emailService; // <--- ADICIONADO AQUI

        public AuthController(IAuthService authService, 
                              UserManager<User> userManager, 
                              AppDbContext context,
                              IPersonService personService,
                              IEmailService emailService) // <--- ADICIONADO NO CONSTRUTOR
        {
            _authService = authService;
            _userManager = userManager;
            _context = context;
            _personService = personService;
            _emailService = emailService; // <--- INICIALIZADO AQUI
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
                                        .AsNoTracking() // Boa prática
                                        .FirstOrDefaultAsync(c => c.UserId == user.Id);
            
            // Dados do Perfil
            var person = await _context.People
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(p => p.UserId == user.Id);

            // 3. Retorna JSON formatado corretamente
            return Ok(new
            {
                token = result.Token, 
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = person != null ? person.Name : user.UserName,
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

        [HttpPost("register-full")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterFull([FromBody] RegisterFullUserDto model)
        {
            // 1. Validação do Tipo
            string roleToAssign = model.UserType.ToUpper();
            var validRoles = new[] { "CLIENT", "SUPPLIER", "CARRIER" };
            if (!validRoles.Contains(roleToAssign))
            {
                return BadRequest(new ErrorResponseDto { Message = "Tipo de usuário inválido." });
            }

            // 2. Gerar Senha Aleatória Segura
            string accessCode = GenerateSecurePassword(); // A "senha" provisória

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 3. Criar Usuário
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                // Cria com a senha gerada
                var createResult = await _userManager.CreateAsync(user, accessCode);

                if (!createResult.Succeeded)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Message = "Erro ao criar usuário",
                        Errors = createResult.Errors.Select(e => e.Description).ToArray()
                    });
                }

                // 4. Vincular Role
                var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
                if (!roleResult.Succeeded) throw new Exception("Erro ao vincular perfil de acesso.");

                // 5. Criar Perfil (Person)
                await _personService.CreateOrUpdateProfile(user.Id, model.ProfileData);

                // 6. Confirma a transação no banco
                await transaction.CommitAsync();

                // -----------------------------------------------------------
                // 7. ENVIO DE E-MAIL (Só envia se o banco salvou com sucesso)
                // -----------------------------------------------------------
                var emailSubject = "Bem-vindo ao Bit System - Seu Código de Acesso";
                var emailBody = $@"
                    <h2>Cadastro Realizado com Sucesso!</h2>
                    <p>Olá, <strong>{model.ProfileData.Name}</strong>.</p>
                    <p>Seu cadastro foi realizado em nossa plataforma como {roleToAssign}.</p>
                    <hr>
                    <p>Para realizar seu primeiro acesso, utilize o e-mail cadastrado e o código abaixo:</p>
                    <h3 style='background-color: #f3f3f3; padding: 10px; display: inline-block;'>{accessCode}</h3>
                    <p><strong>Importante:</strong> Este é um código de acesso provisório. Você será solicitado a criar uma nova senha ao entrar.</p>
                    <hr>
                    <p>Equipe Bit System</p>";

                // Dispara o e-mail
                try 
                {
                    await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);
                }
                catch
                {
                    // Opcional: Logar erro de envio de email. 
                }

                return Ok(new { message = $"Cadastro realizado! O código de acesso foi enviado para {model.Email}." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // === MELHORIA PARA DEBUG ===
                // Vamos descer até a raiz do erro para saber o que o banco recusou
                var innerMessage = ex.InnerException?.Message ?? "";
                
                // Se houver mais níveis de erro interno, pegamos o último
                var deepInner = ex.InnerException;
                while (deepInner?.InnerException != null) 
                {
                    deepInner = deepInner.InnerException;
                    innerMessage = deepInner.Message;
                }
                // ============================

                return BadRequest(new ErrorResponseDto
                {
                    Message = "Erro ao realizar cadastro completo.",
                    Errors = new[] { ex.Message, "Detalhe técnico: " + innerMessage }
                });
            }
        }

        private string GenerateSecurePassword()
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "1234567890";
            const string special = "!@#$%^&*()_-+=";

            var random = new Random();
            
            // Garante pelo menos um caractere de cada tipo exigido pelo Identity
            var passwordChars = new List<char>
            {
                lower[random.Next(lower.Length)],
                upper[random.Next(upper.Length)],
                digits[random.Next(digits.Length)],
                special[random.Next(special.Length)]
            };

            // Preenche o resto até chegar em 10 caracteres
            const string allChars = lower + upper + digits + special;
            while (passwordChars.Count < 10)
            {
                passwordChars.Add(allChars[random.Next(allChars.Length)]);
            }

            // Embaralha para não ficar previsível
            return new string(passwordChars.OrderBy(x => random.Next()).ToArray());
        }
    }
}