using Microsoft.EntityFrameworkCore;

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

        // CITIRE (Read)
        Task<T?> GetByIdAsync(int id); 
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // OPERAȚII (Create, Update, Delete)
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);

        // Salvarea modificărilor, esențială dacă nu o faci în metodele CRUD individuale
        Task<int> SaveChangesAsync();
    }

}
