using apiBit.Models;
using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Data; // <--- ADICIONADO: Necessário para acessar o AppDbContext
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore; // <--- ADICIONADO: Necessário para .Include, .AsNoTracking, .ToListAsync

namespace apiBit.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;

        public AuthService(UserManager<User> userManager, 
                           SignInManager<User> signInManager, 
                           IConfiguration configuration,
                           IEmailService emailService,
                           AppDbContext context) // <--- ADICIONADO: Injeção no construtor
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
            _context = context; // <--- ADICIONADO: Atribuição
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
            // 1. Valida Login
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            if (!result.Succeeded) return null;

            // 2. Busca Usuário e Roles
            var user = await _userManager.FindByEmailAsync(model.Email);
            var userRoles = await _userManager.GetRolesAsync(user);

            // 3. Busca a Empresa (Para preencher o CompanyDto)
            var company = await _context.Companies
                                        .AsNoTracking()
                                        .Include(c => c.Plan) // Inclui o plano se precisar do nome
                                        .FirstOrDefaultAsync(c => c.UserId == user.Id);
            
            // 4. Gera Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("id", user.Id)
            };

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

            // 5. Monta Resposta Final
            return new LoginResponseDto
            {
                Token = tokenString,
                User = new UserDetailDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName
                },
                Roles = userRoles.Select(r => new AccessDto { Type = "Role", Value = r }).ToList(),
                
                // Preenche a Empresa (Se existir)
                Company = company != null ? new CompanyDto 
                {
                    Id = company.Id,
                    Name = company.Name,
                    Document = company.Document, // Assumindo que tem esse campo
                    // Preencha os outros campos do CompanyDto aqui...
                } : null,

                // Preenche os Menus
                Menus = await GetMenusForCompanyPlan(user.Id)
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

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            return result;
        }

        private async Task<List<MenuDto>> GetMenusForCompanyPlan(string userId)
        {
            // 1. Achar a empresa e o ID do Plano
            var company = await _context.Companies
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null) return new List<MenuDto>();

            // 2. TENTATIVA INTELIGENTE: Filtrar pelo plano
            // Como não tenho o código da sua classe 'Plan' e 'AllowedApps', 
            // vou manter a busca segura (trazer todos e montar a árvore).
            // Se você quiser filtrar exato, precisaremos cruzar os IDs.
            
            // Busca ApplicationMenu e Filhos
            var menusFromDb = await _context.ApplicationMenus
                                            .AsNoTracking()
                                            .Include(m => m.SubMenus)
                                            .OrderBy(m => m.Title)
                                            .ToListAsync();

            // 3. Transformar em DTO
            var menuDtos = new List<MenuDto>();

            foreach (var menu in menusFromDb)
            {
                var menuDto = new MenuDto
                {
                    Title = menu.Title,
                    Route = menu.Route,
                    Icon = menu.Icon,
                    Description = menu.Description,
                    ApplicationId = menu.ApplicationId,
                    
                    // IMPORTANTE: Mapear a lista Items para o PrimeNG entender
                    Items = menu.SubMenus.Select(sub => new MenuDto
                    {
                        Title = sub.Title,
                        Route = sub.Route,
                        Icon = sub.Icon,
                        ApplicationId = menu.ApplicationId,
                        Items = new List<MenuDto>() // Submenu do submenu (vazio por enquanto)
                    }).ToList()
                };
                menuDtos.Add(menuDto);
            }

            return menuDtos;
        }
    }
}