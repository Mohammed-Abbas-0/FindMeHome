using FindMeHome.Models;

namespace FindMeHome.Repositories.AbstractionLayer
{
    public interface IUnitOfWork : IDisposable
    {
        IRepositories<RealEstate> RealEstates { get; }
        IRepositories<RealEstateImage> RealEstateImages { get; }
        IRepositories<Furniture> Furnitures { get; }
        IRepositories<Wishlist> Wishlists { get; }
        IRepositories<Review> Reviews { get; }
        IRepositories<PropertyLike> PropertyLikes { get; }
        Task<int> CompleteAsync();
    }
}
