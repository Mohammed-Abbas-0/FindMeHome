using System.ComponentModel.DataAnnotations;

namespace FindMeHome.Dtos
{
    public record RealEstateDto
    {
        [Required(ErrorMessage = "العنوان مطلوب")]
        public string? Title { get; init; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(1000, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من 1000")]
        public decimal? Price { get; init; }

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string? City { get; init; }

        public string? Address { get; init; }

        [Required(ErrorMessage = "المساحة مطلوبة")]
        [Range(10, double.MaxValue, ErrorMessage = "المساحة يجب أن تكون أكبر من 10 م²")]
        public double? Area { get; init; }

        [Required(ErrorMessage = "نوع العرض مطلوب")]
        public string? IsForSale { get; init; }

        public bool CanBeFurnished { get; init; }

        [Required(ErrorMessage = "نوع الوحدة مطلوب")]
        public string? UnitType { get; init; }

        public string? LocationAddress { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }

        public List<FurnitureDto>? Furnitures { get; init; }
        public List<IFormFile>? Images { get; init; }
    }

    public record FurnitureDto
    (
        string? Name,
        decimal? Price,
        IFormFile? Image
    );

    public record RealEstateImageDto
    (
        int Id,
        string FileName,
        string FilePath
    );


}
