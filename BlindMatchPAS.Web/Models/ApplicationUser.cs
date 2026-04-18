using Microsoft.AspNetCore.Identity;

namespace BlindMatchPAS.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}