using FindMeHome.Dtos;
using FindMeHome.Models;

namespace FindMeHome.Services.Abstraction
{
    public interface ICraftsmanService
    {
        Task<IEnumerable<Craftsman>> GetAllAsync();
        Task<bool> AddAsync(CraftsmanDto dto);
    }
}
