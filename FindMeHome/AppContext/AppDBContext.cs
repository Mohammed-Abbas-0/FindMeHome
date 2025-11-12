using FindMeHome.Models;
using Microsoft.EntityFrameworkCore;

namespace FindMeHome.AppContext
{
    public class AppDBContext : DbContext
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

    }
}
