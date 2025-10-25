using Data.SDK.Repository;

using Microsoft.EntityFrameworkCore;

using ProcessImage.Entities;
using ProcessImage.Repository.Interfaces;

namespace ProcessImage.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
      /*  private readonly IRepository<TipProcesare> _tipProcesareRepositroy;
        private readonly IRepository<Subscription> _suscriptionRepositroy;

        public SubscriptionRepository(IRepository<TipProcesare> tipProcesareRepositroy, IRepository<Subscription> suscriptionRepositroy)
        {
            _tipProcesareRepositroy = tipProcesareRepositroy;
            _suscriptionRepositroy = suscriptionRepositroy;
        }

        public async Task<IEnumerable<Subscription>> GetBySubscriptieIdAsync(long subscriptieId)
        {
            return await _suscriptionRepositroy
                .Where(s => s.Id == subscriptieId)
                .ToListAsync();
        }

        public async Task<Subscription> GetByIdAsync(long id)
        {
            return await _suscriptionRepositroy.FindAsync(id);
        }

        public async Task<IEnumerable<SubscripteProcesare>> GetAllSubscripteProcesareAsync()
        {
            return await _suscriptionRepositroy.ToListAsync();
        }

        public async Task<SubscripteProcesare> GetSubscripteProcesareByIdAsync(long id)
        {
            return await _suscriptionRepositroy.FindAsync(id);
        }

        public async Task<IEnumerable<TipProcesare>> GetAllTipProcesareAsync()
        {
            return await _suscriptionRepositroy.ToListAsync();
        }*/
    }
}

