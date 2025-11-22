using FindMeHome.AppContext;
using FindMeHome.Models;
using Microsoft.EntityFrameworkCore;

namespace FindMeHome.Data
{
    public static class CraftsmanSeeder
    {
        public static async Task SeedAsync(AppDBContext context)
        {
            if (!await context.Craftsmen.AnyAsync())
            {
                var craftsmen = new List<Craftsman>
                {
                    new Craftsman { Name = "مبيض محارة", Profession = "مبيض محارة", PhoneNumber = "01275863463" },
                    new Craftsman { Name = "جبسن بورد", Profession = "جبسن بورد", PhoneNumber = "01066804230" },
                    new Craftsman { Name = "رخام", Profession = "رخام", PhoneNumber = "01097723389" },
                    new Craftsman { Name = "كهربائي", Profession = "كهربائي", PhoneNumber = "01159623039" },
                    new Craftsman { Name = "الوميتال", Profession = "الوميتال", PhoneNumber = "01069708026" },
                    new Craftsman { Name = "نجار باب وشباك", Profession = "نجار باب وشباك", PhoneNumber = "01005386324" },
                    new Craftsman { Name = "بلاط و سيراميك", Profession = "بلاط و سيراميك", PhoneNumber = "01096658575" },
                    new Craftsman { Name = "لحام حديد و ابواب حديد", Profession = "لحام حديد", PhoneNumber = "01006759504" },
                    new Craftsman { Name = "نقاش", Profession = "نقاش", PhoneNumber = "01111663268" }
                };

                await context.Craftsmen.AddRangeAsync(craftsmen);
                await context.SaveChangesAsync();
            }
        }
    }
}
