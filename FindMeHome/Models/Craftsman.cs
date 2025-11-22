using System.ComponentModel.DataAnnotations;

namespace FindMeHome.Models
{
    public class Craftsman
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يرجى إدخال الاسم")]
        public string Name { get; set; }

        [Required(ErrorMessage = "يرجى إدخال المهنة")]
        public string Profession { get; set; }

        [Required(ErrorMessage = "يرجى إدخال رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
