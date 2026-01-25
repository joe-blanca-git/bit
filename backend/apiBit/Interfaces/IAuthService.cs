using apiBit.DTOs;
using Microsoft.AspNetCore.Identity;

namespace apiBit.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUser(RegisterUserDto model);
    }
}