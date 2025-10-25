using Data.SDK.Repository;

using Microsoft.EntityFrameworkCore;

using ProcessImage.Entities;
using ProcessImage.Repository.Interfaces;

namespace ProcessImage.Repository
{
    public class UtilizatorRepository : IUtilizatorRepository
    {
      /*  private readonly IRepository<Utilizator> _utilizatorRepositroy;
        public UtilizatorRepository(IRepository<Utilizator> utilizatorRepositroy)
        {
            _utilizatorRepositroy = utilizatorRepositroy;
        }

        public async Task<Utilizator> GetByEmailAsync(string email)
        {
            return await _utilizatorRepositroy.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Utilizator> GetByIdAsync(int id)
        {
            return await _utilizatorRepositroy.FindAsync(id);
        }

        public async Task<Utilizator> CreateAsync(Utilizator utilizator)
        {
            _utilizatorRepositroy.Add(utilizator);
            return utilizator;
        }

        public async Task<Utilizator> UpdateAsync(Utilizator utilizator)
        {
            _utilizatorRepositroy.Update(utilizator);
            return utilizator;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _utilizatorRepositroy.AnyAsync(u => u.Email == email);
        }*/
    }
}
