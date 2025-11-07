using System.ComponentModel.DataAnnotations.Schema;

namespace FindMeHome.Models
{
    public class Review
    {
        public int Id { get; set; }                        // رقم المراجعة (Primary Key)
        public int RealEstateId { get; set; }              // المفتاح الأجنبي للعقار
        [ForeignKey(nameof(RealEstateId))]
        public RealEstate RealEstate { get; set; } = null!;

        public required string UserName { get; set; }      // اسم المستخدم اللي كتب التعليق
        public required string Comment { get; set; }       // نص التعليق
        public int Rating { get; set; }                    // التقييم (من 1 إلى 5 نجوم)

        public DateTime CreatedAt { get; set; } = DateTime.Now; // تاريخ كتابة التعليق
    }
}
