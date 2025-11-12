using FindMeHome.AppContext;
using FindMeHome.Models;
using FindMeHome.Repositories.AbstractionLayer;

namespace FindMeHome.Repositories.ImplementationLayer
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDBContext _context;
        public IRepositories<RealEstate> RealEstates { get; }
        public IRepositories<RealEstateImage> RealEstateImages { get; }
        public IRepositories<Furniture> Furnitures { get; }
        public IRepositories<Wishlist> Wishlists { get; }
        public IRepositories<Review> Reviews { get; }

        public UnitOfWork(AppDBContext context)
        {
            _context = context;
            RealEstates = new Repositories<RealEstate>(_context);
            RealEstateImages = new Repositories<RealEstateImage>(_context);
            Furnitures = new Repositories<Furniture>(_context);
            Wishlists = new Repositories<Wishlist>(_context);
            Reviews = new Repositories<Review>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
