using SixLabors.ImageSharp;
namespace ProcessImage.Services.Interface
{
    public interface IImageProcessingService
    {
        Task SaveProcessingRecord(long imagineId, long tipProcesareId, string status);
        Task<long> SaveImageAsync(IFormFile file, long utilizatorId);
        Task<byte[]> ProcessAndSaveImageAsync(IFormFile image, long utilizatorId, long tipProcesareId, Func<Image, Task> processFunc);
    }
}
