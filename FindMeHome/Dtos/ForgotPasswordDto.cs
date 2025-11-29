using System.ComponentModel.DataAnnotations;

namespace FindMeHome.Dtos
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
