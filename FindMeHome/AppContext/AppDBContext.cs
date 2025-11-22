using FindMeHome.Models;
using Microsoft.EntityFrameworkCore;
// IMPORTANT: DO NOT CHANGE THIS TO Microsoft.AspNet.Identity.EntityFramework
// We are using ASP.NET Core, so we MUST use Microsoft.AspNetCore.Identity.EntityFrameworkCore
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FindMeHome.AppContext
{
    public class AppDBContext : IdentityDbContext<ApplicationUser>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        // الجداول
        public DbSet<RealEstate> RealEstates { get; set; }
        public DbSet<RealEstateImage> RealEstateImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Furniture> Furnitures { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Craftsman> Craftsmen { get; set; }

    }
}
