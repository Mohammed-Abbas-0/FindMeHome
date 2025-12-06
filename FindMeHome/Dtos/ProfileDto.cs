using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using FindMeHome.Enums;

namespace FindMeHome.Dtos
{
    public class ProfileDto
    {
        [Required(ErrorMessage = "RequiredField")]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "RequiredField")]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

        [Display(Name = "WhatsAppNumber")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "AboutMe")]
        public string? Bio { get; set; }

        [Display(Name = "ProfilePicture")]
        public IFormFile? ProfilePicture { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public VerificationStatus VerificationStatus { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Username")]
        public string? Username { get; set; }
    }
}
