using System.ComponentModel.DataAnnotations.Schema;

namespace FindMeHome.Models
{
    public class Wishlist
    {
        public int Id { get; set; }                            // رقم القائمة (Primary Key)
        public int RealEstateId { get; set; }                  // المفتاح الأجنبي للعقار
        [ForeignKey(nameof(RealEstateId))]
        public RealEstate RealEstate { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;    // معرف المستخدم (مؤقت - يمكن ربطه بجدول المستخدمين لاحقًا)
        public DateTime AddedAt { get; set; } = DateTime.Now;  // تاريخ الإضافة
    }
}

