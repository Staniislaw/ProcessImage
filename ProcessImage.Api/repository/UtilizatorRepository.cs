using Microsoft.EntityFrameworkCore;

using ProcessImage.Entities;
using ProcessImage.Repository.Interfaces;

namespace ProcessImage.Repository
{
    public class UtilizatorRepository : IUtilizatorRepository
    {
        private readonly PpawLab02Context _context;

        public UtilizatorRepository(PpawLab02Context context)
        {
            _context = context;
        }

        public async Task<Utilizator> GetByEmailAsync(string email)
        {
            return await _context.Utilizators.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Utilizator> GetByIdAsync(int id)
        {
            return await _context.Utilizators.FindAsync(id);
        }

        public async Task<Utilizator> CreateAsync(Utilizator utilizator)
        {
            _context.Utilizators.Add(utilizator);
            await _context.SaveChangesAsync();
            return utilizator;
        }

        public async Task<Utilizator> UpdateAsync(Utilizator utilizator)
        {
            _context.Utilizators.Update(utilizator);
            await _context.SaveChangesAsync();
            return utilizator;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.Utilizators.AnyAsync(u => u.Email == email);
        }
    }
}
