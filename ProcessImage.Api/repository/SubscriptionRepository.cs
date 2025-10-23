using Microsoft.EntityFrameworkCore;

using ProcessImage.Entities;
using ProcessImage.Repository.Interfaces;

namespace ProcessImage.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly PpawLab02Context _context;

        public SubscriptionRepository(PpawLab02Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subscription>> GetBySubscriptieIdAsync(long subscriptieId)
        {
            return await _context.Subscriptions
                .Where(s => s.Id == subscriptieId)
                .ToListAsync();
        }

        public async Task<Subscription> GetByIdAsync(long id)
        {
            return await _context.Subscriptions.FindAsync(id);
        }

        public async Task<IEnumerable<SubscripteProcesare>> GetAllSubscripteProcesareAsync()
        {
            return await _context.SubscripteProcesares.ToListAsync();
        }

        public async Task<SubscripteProcesare> GetSubscripteProcesareByIdAsync(long id)
        {
            return await _context.SubscripteProcesares.FindAsync(id);
        }

        public async Task<IEnumerable<TipProcesare>> GetAllTipProcesareAsync()
        {
            return await _context.TipProcesares.ToListAsync();
        }
    }
}

