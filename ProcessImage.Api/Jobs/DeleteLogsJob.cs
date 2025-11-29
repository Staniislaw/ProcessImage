using ProcessImage.Services.Interface;

using Quartz;
using System;
using System.Threading.Tasks;

namespace ProcessImage.Jobs
{
    public class DeleteLogsJob : IJob
    {
        private readonly ILogCleanupService _cleanupService;
        public DeleteLogsJob(ILogCleanupService cleanupService)
        {
            _cleanupService = cleanupService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"[{DateTime.Now}] Job Quartz triggered.");
            await _cleanupService.DeleteOldLogsAsync();
        }
    }

}
