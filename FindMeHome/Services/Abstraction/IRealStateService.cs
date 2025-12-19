using FindMeHome.Dtos;
using FindMeHome.Enums;

namespace FindMeHome.Services.Abstraction
{
    public interface IRealStateService
    {
        Task<ResultDto> CreateAsync(CreateRealEstateDto realStateDto, string userId);
        Task<RealEstateDto?> GetByIdAsync(int id);
        Task<PagedResultDto<RealEstateDto>> GetAllAsync(int page = 1, int pageSize = 50);
        Task<List<RealEstateDto>> GetPendingPropertiesAsync();
        Task<List<RealEstateDto>> GetByUserIdAsync(string userId);
        Task<PagedResultDto<RealEstateDto>> SearchAsync(string? query, decimal? minPrice, decimal? maxPrice, double? minArea, double? maxArea, int? rooms, int? bathrooms, string? city, string? neighborhood, UnitType? unitType, bool? isFurnished, string? location = null, int page = 1, int pageSize = 50);
        Task<ResultDto> AddToWishlistAsync(int realEstateId, string userId);
        Task<ResultDto> RemoveFromWishlistAsync(int realEstateId, string userId);
        Task<bool> IsInWishlistAsync(int realEstateId, string userId);
        Task<List<RealEstateDto>> GetWishlistAsync(string userId);
        Task<ResultDto> LikePropertyAsync(int realEstateId, string userId);
        Task<ResultDto> UnlikePropertyAsync(int realEstateId, string userId);
        Task<bool> IsLikedByUserAsync(int realEstateId, string userId);
        Task<int> GetLikesCountAsync(int realEstateId);
        Task<ResultDto> UpdateAsync(int id, CreateRealEstateDto dto, string userId);
        Task<ResultDto> UpdateStatusAsync(int id, PropertyStatus status);
        Task<ResultDto> DeleteAsync(int id, string userId);
        Task<List<LocationSuggestionDto>> GetLocationsAsync(string term);
    }
}
