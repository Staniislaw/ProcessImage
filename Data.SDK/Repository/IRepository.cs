using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Data.SDK.Repository
{
    public interface IRepository<T> where T : class
    {
        // PROPRIETĂȚI
        DbContext Context { get; }
        IQueryable<T> Query { get; }
        DbSet<T> Set { get; }

        // CITIRE (Read) - fără includes
     
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // CITIRE (Read) - cu includes
        Task<IList<T>> GetWhereIncludeAsync(Expression<Func<T, bool>> match, bool asNoTracking = true, params Expression<Func<T, object>>[] includes);
        Task<T?> GetSingleWhereIncludeAsync(Expression<Func<T, bool>> match, bool asNoTracking = true, params Expression<Func<T, object>>[] includes);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null);
        Task<IEnumerable<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? includes = null);

        // OPERAȚII (Create, Update, Delete)
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        void Remove(T entity);

        // Salvarea modificărilor
        Task<int> SaveChangesAsync();
    }
}
