using Data.SDK.Repository;
using Microsoft.EntityFrameworkCore;

using ProcessImage.Entities;
using ProcessImage.repository.Interface;

namespace ProcessImage.repository
{
    public class Accesor : BaseRepository<Subscription, PpawLab02Context>, IAccesor
    {
        public Accesor(PpawLab02Context context) : base(context)
        {
        }
        public async Task<IEnumerable<Subscription>> GetSubscriptionsByUserIdAsync(int userId)
        {
            return await Context.Set<Subscription>()
                                 .Where(s => s.Id == userId)
                                 .ToListAsync();
        }
        public async Task<IEnumerable<SubscripteProcesare>> GetAllSubscripteProcesareAsync()
        {
            return await Context.Set<SubscripteProcesare>().ToListAsync();
        }

        public async Task<SubscripteProcesare?> GetSubscripteProcesareByIdAsync(int id)
        {
            return await Context.Set<SubscripteProcesare>().FindAsync(id);
        }


    }
}
