using apiBit.API.Models;
using apiBit.DTOs;
using apiBit.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace apiBit.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;

        public AuthService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> RegisterUser(RegisterUserDto model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {            
                await _userManager.AddToRoleAsync(user, "Owner");
            }

            return result;
        }
    }
}