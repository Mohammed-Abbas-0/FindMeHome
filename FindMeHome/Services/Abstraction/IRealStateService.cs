using FindMeHome.Dtos;
using FindMeHome.Enums;

namespace FindMeHome.Services.Abstraction
{
    public interface IRealStateService
    {
        Task<ResultDto> CreateAsync(CreateRealEstateDto realStateDto);
        Task<RealEstateDto?> GetByIdAsync(int id);
        Task<List<RealEstateDto>> GetAllAsync();
        Task<List<RealEstateDto>> SearchAsync(string? query, decimal? minPrice, decimal? maxPrice, double? minArea, double? maxArea, int? rooms, int? bathrooms, string? city, string? neighborhood, UnitType? unitType, bool? isFurnished);
        Task<ResultDto> AddToWishlistAsync(int realEstateId, string userId);
        Task<ResultDto> RemoveFromWishlistAsync(int realEstateId, string userId);
        Task<bool> IsInWishlistAsync(int realEstateId, string userId);
        Task<List<RealEstateDto>> GetWishlistAsync(string userId);
    }
}
