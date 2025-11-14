using FindMeHome.Enums;
using System.ComponentModel.DataAnnotations;

namespace FindMeHome.Dtos
{
    public class CreateRealEstateDto
    {
        [Required(ErrorMessage = "العنوان مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "العنوان التفصيلي مطلوب")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        public string Neighborhood { get; set; } = string.Empty;

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(1, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من صفر")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "المساحة مطلوبة")]
        [Range(1, double.MaxValue, ErrorMessage = "المساحة يجب أن تكون أكبر من صفر")]
        public double Area { get; set; }

        [Required(ErrorMessage = "نوع العرض مطلوب")]
        public ApartmentType ApartmentType { get; set; }

        public bool CanBeFurnished { get; set; }

        public List<FurnitureDto>? Furnitures { get; set; } = new List<FurnitureDto>();

        [Required(ErrorMessage = "عدد الغرف مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "عدد الغرف يجب أن يكون أكبر من صفر")]
        public int Rooms { get; set; }

        [Required(ErrorMessage = "عدد الحمامات مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "عدد الحمامات يجب أن يكون أكبر من صفر")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "نوع الوحدة مطلوب")]
        public UnitType UnitType { get; set; }

        [Required(ErrorMessage = "رقم الواتساب مطلوب")]
        [Phone(ErrorMessage = "رقم الواتساب غير صحيح")]
        public string WhatsAppNumber { get; set; } = string.Empty;

        public List<IFormFile>? Images { get; set; } = new List<IFormFile>();

        public string? LocationAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}


