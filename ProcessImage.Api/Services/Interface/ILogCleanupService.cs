namespace ProcessImage.Services.Interface
{
    public interface ILogCleanupService
    {
        Task DeleteOldLogsAsync();
    }
}
