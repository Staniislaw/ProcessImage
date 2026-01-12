using Data.SDK.Repository;

using log4net;

using ProcessImage.Entities;
using ProcessImage.Helpers;
using ProcessImage.Services.Interface;

namespace ProcessImage.Services
{
    public class ImageCleanupService : IImageCleanupService
    {
        private readonly IRepository<Imagine> _imagineRepository;
        private readonly int _daysToKeep = 30; // Păstrează imaginile din ultimele 30 zile
        private readonly ILog _logger;


        public ImageCleanupService(IRepository<Imagine> imagineRepository)
        {
            _imagineRepository = imagineRepository;
            _logger = LoggerAppender.GetLoggerForApplicationType(
            LoggerAppender.LogApplication.ImageCleanup,
            0
        );

        }
        public async Task DeleteOldImagesAsync()
        {
            try
            {
                _logger.Info($"Începe curățarea imaginilor vechi...");

                string imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "imagini");

                if (!Directory.Exists(imagesPath))
                {
                    _logger.Warn($"Folderul {imagesPath} nu există.");
                    return;
                }

                DateTime cutoffDate = DateTime.Now.AddDays(-_daysToKeep);
                _logger.Info($"Șterge imagini mai vechi de {cutoffDate:dd/MM/yyyy HH:mm}");

                var oldImages = await _imagineRepository.FindAsync(i => i.DataIncarcarii < cutoffDate);

                _logger.Info($"Găsite {oldImages.Count()} imagini vechi pentru ștergere.");

                int deletedFiles = 0;
                int deletedRecords = 0;
                int failedDeletions = 0;

                foreach (var imagine in oldImages)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(imagine.CaleFisier))
                        {
                            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagine.CaleFisier);

                            if (File.Exists(fullPath))
                            {
                                File.Delete(fullPath);
                                deletedFiles++;
                                _logger.Info($"Șters fișier: {imagine.CaleFisier}");
                            }
                            else
                            {
                                _logger.Warn($"Fișier nu există pe disc: {fullPath}");
                            }
                        }

                        await _imagineRepository.DeleteAsync(imagine);
                        deletedRecords++;
                    }
                    catch (Exception ex)
                    {
                        failedDeletions++;
                        _logger.Error($"Eroare la ștergerea imaginii ID {imagine.Id}: {ex.Message}");
                    }
                }

                // Șterge folder-ele goale
                await CleanupEmptyUserFolders(imagesPath);

                _logger.Info($"Curățare finalizată: {deletedFiles} fișiere șterse, {deletedRecords} înregistrări șterse, {failedDeletions} eșuate.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Eroare critică la curățarea imaginilor: {ex.Message}");
                _logger.Error($"Stack Trace: {ex.StackTrace}");
            }
        }

        private async Task CleanupEmptyUserFolders(string imagesPath)
        {
            try
            {
                var userFolders = Directory.GetDirectories(imagesPath);
                int deletedFolders = 0;

                foreach (var folder in userFolders)
                {
                    if (!Directory.EnumerateFileSystemEntries(folder).Any())
                    {
                        Directory.Delete(folder);
                        deletedFolders++;
                        _logger.Info($"Șters folder gol: {Path.GetFileName(folder)}");
                    }
                }

                if (deletedFolders > 0)
                {
                    _logger.Info($"Total folder-e goale șterse: {deletedFolders}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Eroare la ștergerea folder-elor goale: {ex.Message}");
            }

            await Task.CompletedTask;
        }
    }
}
