using Microsoft.AspNetCore.Identity;

namespace apiBit.Models
{
    public class User: IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}