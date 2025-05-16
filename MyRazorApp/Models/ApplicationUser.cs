using Microsoft.AspNetCore.Identity;

namespace MyRazorApp.Models
{
    public class AppIdentityUser : IdentityUser
    {
        // Identity’ye özel alanlar buraya eklenebilir
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
