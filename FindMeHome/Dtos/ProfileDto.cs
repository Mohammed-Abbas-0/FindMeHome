using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FindMeHome.Dtos
{
    public class ProfileDto
    {
        [Required]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

        [Display(Name = "PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Display(Name = "ProfilePicture")]
        public IFormFile? ProfilePicture { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string? Email { get; set; }
        public string? Username { get; set; }
    }
}
