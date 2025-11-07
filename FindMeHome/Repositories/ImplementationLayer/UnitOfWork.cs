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

        //public IRepository<Comment> Comments { get; }

        public UnitOfWork(AppDBContext context)
        {
            _context = context;
            RealEstates = new Repositories<RealEstate>(_context);
            RealEstateImages = new Repositories<RealEstateImage>(_context);
            //Comments = new Repository<Comment>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
