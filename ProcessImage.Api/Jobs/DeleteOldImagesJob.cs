using log4net;

using ProcessImage.Helpers;
using ProcessImage.Services.Interface;
using Quartz;

namespace ProcessImage.Jobs
{
    public class DeleteOldImagesJob : IJob
    {
        private readonly IImageCleanupService _imageCleanupService;
        private readonly ILog _logger;
        public DeleteOldImagesJob(IImageCleanupService imageCleanupService)
        {
            _imageCleanupService = imageCleanupService;
            _logger = LoggerAppender.GetLoggerForApplicationType(
                LoggerAppender.LogApplication.ImageCleanup,
                0
            );
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.Info("========================================");
            _logger.Info("DeleteOldImagesJob DECLANȘAT");
            _logger.Info("========================================");

            try
            {
                await _imageCleanupService.DeleteOldImagesAsync();
                _logger.Info("DeleteOldImagesJob finalizat cu SUCCES");
            }
            catch (Exception ex)
            {
                _logger.Error($"DeleteOldImagesJob finalizat cu EROARE: {ex.Message}");
            }

            _logger.Info("========================================");
        }
    }
}
