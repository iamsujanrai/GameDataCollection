using Microsoft.AspNetCore.Identity;

namespace GameDataCollection.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
    }
}
