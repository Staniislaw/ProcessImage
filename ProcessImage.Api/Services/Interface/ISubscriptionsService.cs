using ProcessImage.Models.DTO;

namespace ProcessImage.Services.Interface
{
    public interface ISubscriptionsService
    {
        Task<SubscriptionLimitsResponse> GetSubscriptionLimitsAsync(int id);
        Task<List<SubscriptionReportDto>> GetSubscriptionsReportAsync();

    }
}
