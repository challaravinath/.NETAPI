using Microsoft.AspNetCore.Identity;

namespace ProductApi.Auth
{
    public class ApplicationUser : IdentityUser
    {
        public string RefreshToken { get; set; }
        public DateTime? RefreshToikenExpiryTime { get; set; }
    }
}
