namespace ProcessImage.Services.Interface
{
    public interface IBaseService
    {
        int GetUserId();
        int? GetUserIdOrNull();
        string GetUserEmail();
        int GetClaimAsInteger(string claimType);
        Task<(bool CanProcess, string Message, int Used, int Remaining)> CheckProcessingLimitAsync(int userId, long processingTypeId);
        Task LogProcessingAsync(int imagineId, long processingTypeId, string status);
    }

}
