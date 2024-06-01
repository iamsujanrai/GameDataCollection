using System.Linq.Expressions;

namespace GameDataCollection.Repositories
{
    public interface IBaseRepository<T>
    {
        void Delete(T entity);
        T Insert(T entity);
        Task<T> InsertAsync(T entity);
        void Update(T entity);
        void UpdateAsync(T entity);
        void DeleteRange(List<T> entities);
        void InsertRange(List<T> entities);
        void UpdateRange(List<T> entities);
        Task<List<T>> GetAll();
        T GetById(long id);
        IQueryable<T> GetQueryable();

        List<T> GetAll(bool asNoTracking = false);
        List<T> Get(Expression<Func<T, bool>>? filterExpression = null, Expression<Func<T, object>>? orderExpression = null);

        List<T> GetWithMultipleInclude(Expression<Func<T, bool>>? filterExpression = null,
            Expression<Func<T, object>>? orderExpression = null, bool orderDescending = false, string? includeProperties = null, int? skip = null,
            int? take = null, bool asNoTracking = false);
    }
}
