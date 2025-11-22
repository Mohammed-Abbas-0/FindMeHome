using FindMeHome.AppContext;
using FindMeHome.Dtos;
using FindMeHome.Models;
using FindMeHome.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace FindMeHome.Services.Implementation
{
    public class CraftsmanService : ICraftsmanService
    {
        private readonly AppDBContext _context;

        public CraftsmanService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(CraftsmanDto dto)
        {
            try
            {
                var craftsman = new Craftsman
                {
                    Name = dto.Name,
                    Profession = dto.Profession,
                    PhoneNumber = dto.PhoneNumber
                };

                await _context.Craftsmen.AddAsync(craftsman);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Craftsman>> GetAllAsync()
        {
            return await _context.Craftsmen
                                 .OrderByDescending(c => c.CreatedAt)
                                 .ToListAsync();
        }
    }
}
