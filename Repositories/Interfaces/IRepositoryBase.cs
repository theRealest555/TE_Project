using System.Linq.Expressions;

namespace TE_Project.Repositories.Interfaces
{
    public interface IRepositoryBase<T> where T : class
    {
        // Read operations
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "",
            bool trackChanges = false);
            
        Task<T?> GetByIdAsync(object id, string includeProperties = "", bool trackChanges = false);
        
        Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> filter,
            string includeProperties = "",
            bool trackChanges = false);
            
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
        
        // Create, Update, Delete operations
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        
        // Save changes
        Task<int> SaveChangesAsync();
    }
}