using ProcessImage.Services.Interface;

namespace ProcessImage.Services
{
    public class LogCleanupService : ILogCleanupService
    {
        public Task DeleteOldLogsAsync()
        {
            string logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            if (!Directory.Exists(logsPath))
            {
                return Task.CompletedTask;
            }
            var files = Directory.GetFiles(logsPath, "*.log*", SearchOption.TopDirectoryOnly);
            int counter = 0;

            foreach (var file in files)
            {
                var info = new FileInfo(file);
                if (info.LastWriteTime < DateTime.Now.AddDays(-3))
                {
                    try
                    {
                        File.Delete(file);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return Task.CompletedTask;
        }

    }

}
