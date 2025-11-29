using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FindMeHome.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        public bool IsSellerRequest { get; set; }

        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
    }
}
