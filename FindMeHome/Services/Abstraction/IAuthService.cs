using FindMeHome.Dtos;
using System.Threading.Tasks;

namespace FindMeHome.Services.Abstraction
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterDto model);
        Task<AuthModel> GetTokenAsync(LoginDto model);
        Task<string> AddRoleAsync(string roleName); // Helper to add roles if needed
    }
}
