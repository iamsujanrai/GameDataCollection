using GameDataCollection.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Z.BulkOperations;

namespace GameDataCollection.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly UserDbContext _appDbContext;
        public BaseRepository(UserDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public void Delete(T entity)
        {
            _appDbContext.Set<T>().Remove(entity);
            _appDbContext.SaveChanges();
        }

        public void DeleteRange(List<T> entities)
        {
            _appDbContext.Set<T>().RemoveRange(entities);
            _appDbContext.SaveChanges();
        }

        public void BulkInsert(List<T> entities, Action<BulkOperation<T>> options)
        {
            //_appDbContext.BulkInsert(entities, options);
            _appDbContext.SaveChanges();
        }
        public async Task<List<T>> GetAll()
        {
            return await _appDbContext.Set<T>().ToListAsync();
        }

        public T GetById(long id)
        {
            return _appDbContext.Set<T>().Find(id);
        }

        public T Insert(T entity)
        {
            _appDbContext.Set<T>().Add(entity);
            _appDbContext.SaveChanges();
            return entity;
        }
        public async Task<T> InsertAsync(T entity)
        {
            await _appDbContext.Set<T>().AddAsync(entity);
            _appDbContext.SaveChanges();
            return entity;
        }

        public void InsertRange(List<T> entities)
        {
            _appDbContext.Set<T>().AddRange(entities);
            _appDbContext.SaveChanges();
        }
        public void Update(T entity)
        {
            _appDbContext.Set<T>().Update(entity);
            _appDbContext.SaveChanges();
        }
        public async void UpdateAsync(T entity)
        {
            await _appDbContext.Set<T>().SingleUpdateAsync(entity);
            _appDbContext.SaveChanges();
        }

        public void UpdateRange(List<T> entities)
        {
            _appDbContext.Set<T>().UpdateRange(entities);
            _appDbContext.SaveChanges();
        }

        public List<T> GetAll(bool asNoTracking = false)
        {
            return asNoTracking ? _appDbContext.Set<T>().AsNoTracking().ToList() : _appDbContext.Set<T>().ToList();
        }

        public List<T> Get(Expression<Func<T, bool>> filterExpression = null, Expression<Func<T, object>> orderExpression = null)
        {
            IQueryable<T> query = _appDbContext.Set<T>();
            if (filterExpression != null)
                query = query.Where(filterExpression);
            if (orderExpression != null)
                query = query.OrderBy(orderExpression);
            return query.ToList();
        }
        public List<T> GetWithMultipleInclude(Expression<Func<T, bool>> filterExpression = null, Expression<Func<T, object>> orderExpression = null, bool orderDescending = false, string includeProperties = null, int? skip = null,
            int? take = null, bool asNoTracking = false)
        {
            includeProperties ??= string.Empty;
            var query = asNoTracking ? _appDbContext.Set<T>().AsNoTracking() : _appDbContext.Set<T>();

            if (filterExpression != null)
                query = asNoTracking ? query.Where(filterExpression).AsNoTracking() : query.Where(filterExpression);
            query = includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty) => asNoTracking ? current.Include(includeProperty).AsNoTracking() : current.Include(includeProperty));
            if (orderExpression != null)
            {
                query = orderDescending ? query.OrderByDescending(orderExpression) : query.OrderBy(orderExpression);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }
            return query.ToList();
        }

        public Task<T> FirstOrDefaultAsync()
        {
            return _appDbContext.Set<T>().FirstOrDefaultAsync();
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return _appDbContext.Set<T>().FirstOrDefaultAsync(expression);
        }

        public Task<List<T>> GetAllAsync(bool asNoTracking = false)
        {
            return asNoTracking ? _appDbContext.Set<T>().AsNoTracking().ToListAsync() : _appDbContext.Set<T>().ToListAsync();
        }

        public Task<List<T>> GetAsync(Expression<Func<T, bool>> filterExpression = null, Expression<Func<T, object>> orderExpression = null)
        {
            IQueryable<T> query = _appDbContext.Set<T>();

            if (filterExpression != null)
                query = query.Where(filterExpression);

            if (orderExpression != null)
                query = query.OrderBy(orderExpression);
            return query.ToListAsync();
        }

        public Task<List<T>> GetWithMultipleIncludeAsync(Expression<Func<T, bool>> filterExpression = null, Expression<Func<T, object>> orderExpression = null, bool orderDescending = false,
            string includeProperties = null, int? skip = null, int? take = null, bool asNoTracking = false)
        {
            includeProperties ??= string.Empty;
            IQueryable<T> query = _appDbContext.Set<T>();
            if (filterExpression != null)
                query = query.Where(filterExpression);
            query = includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty) => current.Include(includeProperty));


            if (asNoTracking)
                query = query.AsNoTracking();
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }
            if (orderExpression != null)
            {
                query = orderDescending ? query.OrderByDescending(orderExpression) : query.OrderBy(orderExpression);
            }
            return query.ToListAsync();
        }

        public IQueryable<T> GetQueryable()
        {
            return _appDbContext.Set<T>();
        }

        private Expression getExpression(Expression left, Expression right)
        {
            return Expression.Equal(left, right);

        }
    }
}
