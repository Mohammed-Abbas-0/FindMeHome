using System.ComponentModel.DataAnnotations.Schema;

namespace FindMeHome.Models
{
    // PropertyLike Model for Likes System
    public class PropertyLike
    {
        public int Id { get; set; }                            // رقم الإعجاب (Primary Key)
        public int RealEstateId { get; set; }                  // المفتاح الأجنبي للعقار
        [ForeignKey(nameof(RealEstateId))]
        public RealEstate RealEstate { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;     // معرف المستخدم
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public DateTime LikedAt { get; set; } = DateTime.Now;  // تاريخ الإعجاب
    }
}
