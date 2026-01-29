using apiBit.DTOs;
using Microsoft.AspNetCore.Identity;

namespace apiBit.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUser(RegisterUserDto model);
        Task<LoginResponseDto?> Login(LoginUserDto model);
        Task<bool> ForgotPassword(string email);
        Task<IdentityResult> ResetPassword(ResetPasswordDto model);
        Task<IdentityResult> ChangePassword(string email, ChangePasswordDto model);
    }
}