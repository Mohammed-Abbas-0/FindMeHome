using FindMeHome.Enums;

namespace FindMeHome.Models
{
    public class RealEstate
    {
        public int Id { get; set; }                            // رقم العقار (Primary Key)
        public required string Title { get; set; }             // عنوان مختصر للإعلان
        public string? Description { get; set; }               // تفاصيل الإعلان
        public required string Address { get; set; }           // العنوان التفصيلي
        public required string City { get; set; }              // المدينة
        public required string Neighborhood { get; set; }      // المنطقة أو الحي

        public decimal Price { get; set; }                     // 💰 السعر
        public double Area { get; set; }                       // 📏 المساحة (متر مربع)

        //public bool IsForSale { get; set; }                    // للبيع ولا للإيجار
        //public bool IsFurnished { get; set; }                  // مفروش؟
        public ApartmentType ApartmentType { get; set; }

        // لو إيجار فقط يظهر الخيار
        public bool CanBeFurnished { get; set; }

        public List<Furniture>? Furnitures { get; set; }
        public int Rooms { get; set; }                         // عدد الغرف
        public int Bathrooms { get; set; }                     // عدد الحمامات
        public UnitType UnitType { get; set; }                     // سكني أو تجاري (Enum)

        public DateTime CreatedAt { get; set; } = DateTime.Now; // تاريخ الإضافة
        public DateTime? ExpirationDate { get; set; }          // تاريخ انتهاء الإعلان
        public bool IsActive { get; set; } = true;             // حالة الإعلان (نشط أو منتهي)
        public string? WhatsAppNumber { get; set; }            // رقم الواتساب للتواصل
        public ICollection<RealEstateImage> Images { get; set; } = new List<RealEstateImage>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

        // Foreign Key to User
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }


        // 🔗 علاقات مستقبلية (تعليقها حاليًا):
        // public int AgentId { get; set; }                     // المفتاح الأجنبي للوكيل
        // public User Agent { get; set; }                      // العلاقة مع جدول المستخدمين
    }
}
