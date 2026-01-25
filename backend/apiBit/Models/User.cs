using Microsoft.AspNetCore.Identity;

namespace apiBit.API.Models
{
    public class User: IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}