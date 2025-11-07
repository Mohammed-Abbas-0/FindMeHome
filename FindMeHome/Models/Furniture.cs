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

        // علاقة بالـ Apartment اللي القطعة دي تخصها
        public int ApartmentId { get; set; }
        public RealEstate RealEstate { get; set; }
    }

}
