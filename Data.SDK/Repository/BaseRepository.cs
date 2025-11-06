using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace Data.SDK.Repository
{
    public class BaseRepository<T, U> : IRepository<T>
        where T : class
        where U : DbContext
    {
        private readonly U _context;
        private readonly bool _useLazyLoading;

        public Expression<Func<T, bool>> WhereFilter { get; set; }

        public BaseRepository(U context, IConfiguration configuration)
        {
            _context = context;
            _useLazyLoading = configuration.GetValue<bool>("DatabaseSettings:UseLazyLoading");
        }

        public DbContext Context => _context;
        public IQueryable<T> Query => _context.Set<T>().AsQueryable();
        public DbSet<T> Set => _context.Set<T>();

        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task<T?> GetByIdAsync(long id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task<IList<T>> GetWhereIncludeAsync(Expression<Func<T, bool>> match, bool asNoTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = asNoTracking ? _context.Set<T>().AsNoTracking() : _context.Set<T>();
            query = query.Where(match);
            if (!_useLazyLoading && includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.ToListAsync();
        }
        public async Task<T?> GetSingleWhereIncludeAsync(Expression<Func<T, bool>> match, bool asNoTracking = true, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = asNoTracking ? _context.Set<T>().AsNoTracking() : _context.Set<T>();
            query = query.Where(match);
            if (!_useLazyLoading && includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>()
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(predicate);
        }
        
        public async Task<T?> GetAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();
            if (!_useLazyLoading && includes != null)
            {
                query = includes(query);
            }
            return await query.FirstOrDefaultAsync(predicate);
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }
        public async Task<IEnumerable<T>> GetAllAsync(
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();
            if (!_useLazyLoading && includes != null)
            {
                query = includes(query);
            }
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>()
                                 .AsNoTracking()
                                 .Where(predicate)
                                 .ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();
            if (!_useLazyLoading && includes != null)
            {
                query = includes(query);
            }
            return await query.Where(predicate).ToListAsync();
        }
        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await SaveChangesAsync();
        }
        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);

            await _context.SaveChangesAsync();
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}