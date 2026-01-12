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
        private readonly IRepository<TipProcesare> _tipProcesareRepository;
        public ImageProcessingService(
            IRepository<Imagine> imagineRepository,
            IRepository<ProcesareImagine> procesareRepository,
            IRepository<TipProcesare> tipProcesareRepository)
        {
            _imagineRepository = imagineRepository;
            _procesareRepository = procesareRepository;
            _tipProcesareRepository = tipProcesareRepository;
        }
        public async Task<long> SaveProcessedImageAsync(byte[] imageBytes, string originalFileName, long utilizatorId)
        {
            try
            {
                // Definire cale fisier
                string basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "imagini");

                // Creare folder pentru utilkizator
                string userFolderPath = Path.Combine(basePath, utilizatorId.ToString());

                // Veririfca dupa creaza daca nu exista
                if (!Directory.Exists(userFolderPath))
                {
                    Directory.CreateDirectory(userFolderPath);
                }

                // generare nume unic pentru fișier
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
                string fullPath = Path.Combine(userFolderPath, fileName);

                // Salvare imagine pe disk
                await File.WriteAllBytesAsync(fullPath, imageBytes);

                // creare calea relativa pentru salvare în baza de date
                string relativePath = Path.Combine("uploads", "imagini", utilizatorId.ToString(), fileName);

                var imagine = new Imagine
                {
                    Nume = Path.GetFileNameWithoutExtension(originalFileName),
                    Tip = Path.GetExtension(originalFileName),
                    UtilizatorId = utilizatorId,
                    DataIncarcarii = DateTime.Now,
                    CaleFisier = relativePath.Replace("\\", "/") 
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
        public async Task<byte[]> ProcessAndSaveImageAsync(
    IFormFile image,
    long utilizatorId,
    long tipProcesareId,
    Func<Image, Task> processFunc)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Nu a fost selectata nicio imagine.");

            long imagineId = 0;
            try
            {
                byte[] processedImageBytes;

                //Procesare imagine
                using (var img = await Image.LoadAsync(image.OpenReadStream()))
                {
                    await processFunc(img);

                    using var ms = new MemoryStream();
                    await img.SaveAsync(ms, new PngEncoder());
                    processedImageBytes = ms.ToArray();
                }

                // Salvare imagine procesate 
                imagineId = await SaveProcessedImageAsync(processedImageBytes, image.FileName, utilizatorId);

                await SaveProcessingRecord(imagineId, tipProcesareId, ProcessingStatusEnum.Success);

                return processedImageBytes;
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

        public async Task<List<TipProcesare>> GetTipProcesareId()
        {
            var processingTypes = await _tipProcesareRepository.GetAllAsync();
            return processingTypes.ToList();
        }
    }
}
