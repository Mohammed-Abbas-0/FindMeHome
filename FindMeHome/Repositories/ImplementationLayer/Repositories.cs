using FindMeHome.AppContext;
using FindMeHome.Repositories.AbstractionLayer;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FindMeHome.Repositories.ImplementationLayer
{
    public class Repositories<T> : IRepositories<T> where T : class
    {
        protected readonly AppDBContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repositories(AppDBContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            
            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<T?> GetByIdAsync(int id, string? includeProperties = null)
        {
            if (string.IsNullOrWhiteSpace(includeProperties))
                return await _dbSet.FindAsync(id);

            IQueryable<T> query = _dbSet;
            
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
            
            // Try to find by Id using reflection or direct query
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, "Id");
            var constant = System.Linq.Expressions.Expression.Constant(id);
            var equals = System.Linq.Expressions.Expression.Equal(property, constant);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equals, parameter);
            
            return await query.FirstOrDefaultAsync(lambda);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            
            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public void Remove(T entity) => _dbSet.Remove(entity);
    }
}
