using Microsoft.AspNetCore.Identity;

namespace RecamNewBackend.Models
{
    public class User : IdentityUser
    {
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    }
}