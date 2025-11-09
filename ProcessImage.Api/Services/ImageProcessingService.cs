using Data.SDK.Repository;
using ProcessImage.Entities;
using ProcessImage.Services.Interface;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using ProcessImage.Models.Enum;
namespace ProcessImage.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly IRepository<ProcesareImagine> _procesareRepository;
        private readonly IRepository<Imagine> _imagineRepository;
        public ImageProcessingService(
            IRepository<Imagine> imagineRepository,
            IRepository<ProcesareImagine> procesareRepository)
        {
            _imagineRepository = imagineRepository;
            _procesareRepository = procesareRepository;
        }
        public async Task<long> SaveImageAsync(IFormFile file, long utilizatorId)
        {
            try
            {
                var imagine = new Imagine
                {
                    Nume = Path.GetFileNameWithoutExtension(file.FileName),
                    Tip = Path.GetExtension(file.FileName),
                    UtilizatorId = utilizatorId,
                    DataIncarcarii = DateTime.Now,
                    CaleFisier = "" 
                };
                await _imagineRepository.AddAsync(imagine);

                return imagine.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la salvarea imaginii: {ex.Message}");
                throw;
            }
        }
        public async Task SaveProcessingRecord(long imagineId, long tipProcesareId, string status)
        {
            try
            {
                var procesare = new ProcesareImagine
                {
                    ImagineId = imagineId,
                    TipProcesareId = tipProcesareId,
                    Status = status,
                    DataProcesare = DateTime.Now
                };
                await _procesareRepository.AddAsync(procesare);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la salvarea în DB: {ex.Message}");
            }
        }
        public async Task<byte[]> ProcessAndSaveImageAsync(IFormFile image,long utilizatorId,long tipProcesareId,Func<Image, Task> processFunc)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Nu a fost selectata nicio imagine.");
            long imagineId = 0;
            try
            {
                using var img = await Image.LoadAsync(image.OpenReadStream());
                await processFunc(img);
                using var ms = new MemoryStream();
                await img.SaveAsync(ms, new PngEncoder());
                ms.Position = 0;
                imagineId = await SaveImageAsync(image, utilizatorId);
                await SaveProcessingRecord(imagineId, tipProcesareId, ProcessingStatusEnum.Success);

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la procesare: {ex.Message}");
                if (imagineId != 0)
                {
                    try
                    {
                        await SaveProcessingRecord(imagineId, tipProcesareId, ProcessingStatusEnum.Failed);
                    }
                    catch
                    {
                    }
                }


                throw;
            }
        }
    }
}
