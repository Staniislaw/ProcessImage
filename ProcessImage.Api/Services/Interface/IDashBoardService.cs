using log4net.Core;

using Microsoft.AspNetCore.Mvc;

namespace ProcessImage.Services.Interface
{
    public interface IDashBoardService
    {
        Task<IActionResult> DeleteFileAsync(int id, int userId);
        Task<IActionResult> DeleteProcessingDataAsync(int id, int userId);
        Task<IActionResult> GetFilesDataAsync(int userId, int skip, int take);
        Task<IActionResult> GetProcessingDataAsync(int userId, int skip, int take);
        Task<IActionResult> GetStatsAsync(int userId);
    }
}
