using Data.SDK.Repository;

using ProcessImage.Entities;
namespace ProcessImage.repository
{
    public interface IAccesor : IRepository<Subscription>
    {
        Task<IEnumerable<Subscription>> GetSubscriptionsByUserIdAsync(int userId);
        Task<IEnumerable<SubscripteProcesare>> GetAllSubscripteProcesareAsync();
        Task<SubscripteProcesare?> GetSubscripteProcesareByIdAsync(int id);
    }



}
