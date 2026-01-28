using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using apiBit.Interfaces;
using apiBit.DTOs;
using Microsoft.AspNetCore.RateLimiting;

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
            private readonly IAuthService _authService;

            public AuthController(IAuthService authService)
            {
                _authService = authService;
            }

            ///<summary>
            /// Registra um novo usuário na plataforma Bit.
            /// </summary>
            /// <remarks>
            /// Example Request:
            /// 
            ///     POST /api/Auth/Register
            ///     {
            ///         "name": "Jhon Doe",
            ///         "email": "jhondoe@email.com",
            ///         "password" : "PasswordStrong123@",
            ///         "confirmPassword": "PasswordStrong123@"
            ///     }
            /// 
            /// Password Requirements:
            /// - Mínimo 8 caracteres
            /// - Letra Maiúscula e Minúscula
            /// - Caractere Especial (!@#)
            /// </remarks>
            /// <param name="registerDto">Dados do usuário</param>
            /// <returns>Message of Success</returns>
            /// <response code="200">Usuário criado com sucesso</response>
            /// <response code="400">Falha na validação dos dados</response>
            /// <response code="500">Erro interno no servidor</response>
            
            [HttpPost("register")]
            [EnableRateLimiting("AuthLimiter")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
            {
                try
                {
                    if (!ModelState.IsValid)
                        return BadRequest(ModelState);

                    var result = await _authService.RegisterUser(registerDto);

                    if (result.Succeeded)
                    {
                        return Ok(new {message = "Usuário cadastrado com sucesso!"});
                    }

                    return BadRequest(new ErrorResponseDto
                    {
                        Message = "Erro ao cadastrar usuário",
                        Errors = result.Errors.Select(e => e.Description)
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                    return StatusCode(500, new { message = "Ocorreu um erro interno no servidor. Tente novamente mais tarde."});
                }
            }

            ///<summary>
            /// Obtém token.
            /// </summary>
            /// <remarks>
            /// Example Request:
            /// 
            ///     POST /api/Auth/Login
            ///     {
            ///         "email": "jhondoe@email.com",
            ///         "password" : "PasswordStrong123@"
            ///     }
            /// </remarks>
            /// <param name="loginDto">Credenciais do Usuário</param>
            /// <returns>Message of Success</returns>
            /// <response code="200">Login realizado com sucesso</response>
            /// <response code="400">Falha na validação dos dados</response>
            /// <response code="500">Erro interno no servidor</response>
            [HttpPost("login")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
            {
                var token = await _authService.Login(loginDto);

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "Email ou senha inválidos" });
                }

                return Ok(new { token = token, message = "Login realizado com sucesso!" });
            }
            
    }
}