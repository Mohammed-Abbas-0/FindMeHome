using System.Linq.Expressions;

namespace FindMeHome.Repositories.AbstractionLayer
{
    public interface IRepositories<T> where T : class
    {

        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(int id, string? includeProperties = null);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Remove(T entity);
    }
}
