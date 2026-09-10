using Microsoft.AspNetCore.Identity;

namespace FabricDevelopmentHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Company { get; set; }
    }
}