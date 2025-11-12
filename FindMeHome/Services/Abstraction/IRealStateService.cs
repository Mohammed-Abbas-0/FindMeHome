using FindMeHome.Dtos;

namespace FindMeHome.Services.Abstraction
{
    public interface IRealStateService
    {
        Task<ResultDto> CreateAsync(CreateRealEstateDto realStateDto);
        Task<RealEstateDto?> GetByIdAsync(int id);
        Task<List<RealEstateDto>> GetAllAsync();
        Task<ResultDto> AddToWishlistAsync(int realEstateId, string userId);
        Task<ResultDto> RemoveFromWishlistAsync(int realEstateId, string userId);
        Task<bool> IsInWishlistAsync(int realEstateId, string userId);
        Task<List<RealEstateDto>> GetWishlistAsync(string userId);
    }
}
