using System.ComponentModel.DataAnnotations.Schema;

namespace FindMeHome.Models
{
    public class Furniture
    {
        public int Id { get; set; }

        // اسم القطعة (تلفاز - سرير - تكييف)
        public string Name { get; set; } = string.Empty;

        // السعر الإضافي اللي بيتضاف على الإيجار الشهري
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }

        // علاقة بالـ RealEstate اللي القطعة دي تخصها
        public int RealEstateId { get; set; }
        [ForeignKey(nameof(RealEstateId))]
        public RealEstate RealEstate { get; set; } = null!;
    }

}
