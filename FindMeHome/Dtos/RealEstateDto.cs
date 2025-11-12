using FindMeHome.Enums;
using System.ComponentModel.DataAnnotations;

namespace FindMeHome.Dtos
{
    //public record RealEstateDto
    //{
    //    [Required(ErrorMessage = "العنوان مطلوب")]
    //    public string? Title { get; init; }

    //    [Required(ErrorMessage = "السعر مطلوب")]
    //    public decimal? Price { get; init; }

    //    [Required(ErrorMessage = "المدينة مطلوبة")]
    //    public string? City { get; init; }

    //    public string? Address { get; init; }

    //    [Required(ErrorMessage = "المساحة مطلوبة")]
    //    public double? Area { get; init; }

    //    [Required(ErrorMessage = "نوع العرض مطلوب")]
    //    public string? IsForSale { get; init; }

    //    public bool CanBeFurnished { get; init; }

    //    [Required(ErrorMessage = "نوع الوحدة مطلوب")]
    //    public string? UnitType { get; init; }

    //    public string? LocationAddress { get; init; }
    //    public double? Latitude { get; init; }
    //    public double? Longitude { get; init; }

    //    public List<FurnitureDto> Furnitures { get; init; } = new();
    //    public List<IFormFile>? Images { get; init; } = new();
    //}

    public record RealEstateDto(
        int Id,
        string Title,
        string? Description,
        string Address,
        string City,
        string Neighborhood,
        decimal Price,
        double Area,
        bool IsFurnished,
        ApartmentType ApartmentType,
        bool CanBeFurnished,
        List<FurnitureDto>? Furnitures,
        int Rooms,
        int Bathrooms,
        UnitType UnitType,
        DateTime CreatedAt,
        DateTime? ExpirationDate,
        bool IsActive,
        string? WhatsAppNumber,
        List<RealEstateImageDto>? Images
        //List<ReviewDto>? Reviews
    );
    public record FurnitureDto
    (
        int? Id,
        string? Name,
        decimal? Price,
        string? ImagePath,
        IFormFile? Image
    );

    public record RealEstateImageDto
    (
        int Id,
        string FileName,
        string FilePath
    );


}
