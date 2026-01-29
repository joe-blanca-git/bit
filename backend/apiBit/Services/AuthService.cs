using apiBit.API.Models; // <--- O User provavelmente está aqui
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;     // <--- Ou aqui
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace apiBit.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<User> userManager, 
                           SignInManager<User> signInManager, 
                           IConfiguration configuration,
                           IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<IdentityResult> RegisterUser(RegisterUserDto model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Owner");
            }

            return result;
        }

        public async Task<LoginResponseDto?> Login(LoginUserDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (!result.Succeeded)
            {
                return null;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // --- LÓGICA DE GERAR O TOKEN (INLINE) ---
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            
            // Pega as roles do usuário
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("id", user.Id)
            };

            // Adiciona as roles nas claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            // ----------------------------------------

            return new LoginResponseDto
            {
                Token = tokenString,
                Message = "Login realizado com sucesso!"
            };
        }

        public async Task<bool> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return true; 

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            var frontendUrl = "http://localhost:3000/reset-password"; 
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var link = $"{frontendUrl}?token={encodedToken}&email={email}";

            var message = $@"
                <h2>Recuperação de Senha</h2>
                <p>Clique no link abaixo para redefinir sua senha:</p>
                <a href='{link}'>Redefinir Senha</a>
                <p>Este link expira em breve.</p>";

            await _emailService.SendEmailAsync(email, "Recuperação de Senha - Bit System", message);

            return true;
        }

        public async Task<IdentityResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) 
            {
                return IdentityResult.Failed(new IdentityError { Description = "Erro ao redefinir senha." });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            return result;
        }

        public async Task<IdentityResult> ChangePassword(string email, ChangePasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado." });
            }

            // O Identity já verifica se a 'CurrentPassword' está certa antes de trocar
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            return result;
        }
    }
}