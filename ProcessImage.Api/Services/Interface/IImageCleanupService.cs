namespace ProcessImage.Services.Interface
{
    public interface IImageCleanupService
    {
        Task DeleteOldImagesAsync();
    }
}
