using ProcessImage.Entities;

using SixLabors.ImageSharp;
namespace ProcessImage.Services.Interface
{
    public interface IImageProcessingService
    {
        Task SaveProcessingRecord(long imagineId, long tipProcesareId, string status);
        Task<long> SaveProcessedImageAsync(byte[] imageBytes, string originalFileName, long utilizatorId);
        Task<byte[]> ProcessAndSaveImageAsync(IFormFile image, long utilizatorId, long tipProcesareId, Func<Image, Task> processFunc);
        Task<List<TipProcesare>> GetTipProcesareId();
    }
}
