using System.ComponentModel.DataAnnotations.Schema;

namespace FindMeHome.Models
{
    public class RealEstateImage
    {
        public int Id { get; set; }                       // رقم الصورة (Primary Key)
        public required string ImageUrl { get; set; }     // رابط أو مسار الصورة
        public DateTime UploadedAt { get; set; } = DateTime.Now;  // تاريخ رفع الصورة

        // 🔗 العلاقة مع العقار
        public int RealEstateId { get; set; }             // المفتاح الأجنبي
        [ForeignKey(nameof(RealEstateId))]
        public RealEstate RealEstate { get; set; } = null!;
    }
}
