using FindMeHome.Models;

namespace FindMeHome.Repositories.AbstractionLayer
{
    public interface IUnitOfWork: IDisposable
    {
        IRepositories<RealEstate> RealEstates { get; }
        IRepositories<RealEstateImage> RealEstateImages { get; }
        //IRepositories<Comme> Comments { get; }
        Task<int> CompleteAsync();
    }
}
