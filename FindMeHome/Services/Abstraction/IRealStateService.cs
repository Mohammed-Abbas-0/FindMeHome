using FindMeHome.Dtos;

namespace FindMeHome.Services.Abstraction
{
    public interface IRealStateService
    {
        Task<ResultDto> CreateAsync(RealEstateDto realStateDto);
    }
}
