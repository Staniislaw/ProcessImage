using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Data.SDK.Repository
{
    public class BaseRepository<T, U> : IRepository<T>
        where T : class
        where U : DbContext
    {
        private readonly U _context;
        // Proprietățile tale existente
        public Expression<Func<T, bool>> WhereFilter { get; set; }

        public BaseRepository(U context)
        {
            _context = context;
        }

        // IMPLEMENTARE PROPRIETĂȚI IRepository
        public DbContext Context => _context;

        public IQueryable<T> Query =>
            _context.Set<T>().AsQueryable();

        // Nu este necesară expunerea DbSet, dar o păstrăm pentru coerență
        public DbSet<T> Set => _context.Set<T>();

        // --- IMPLEMENTARE METODE IRepository ---

        // CREATE (Adaugă entitatea în memorie)
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            // Salvarea va fi apelată separat (sau adăugată aici, de ex. await SaveChangesAsync();)
        }

        // READ (Găsește după ID)
        public async Task<T?> GetByIdAsync(int id)
        {
            // Metoda FindAsync este optimizată pentru căutarea după cheia primară
            return await _context.Set<T>().FindAsync(id);
        }

        // READ (Extrage toate elementele)
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Folosește AsNoTracking() dacă nu ai nevoie să modifici entitățile
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        // READ (Extrage elemente pe baza unui predicat)
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>()
                                 .AsNoTracking()
                                 .Where(predicate)
                                 .ToListAsync();
        }

        // UPDATE (Marchează entitatea ca modificată)
        public void Update(T entity)
        {
            _context.Set<T>().Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        // DELETE (Marchează entitatea pentru ștergere)
        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        // Salvarea modificărilor, necesară pentru a aplica toate operațiile CRUD
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }

}
