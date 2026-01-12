using Data.SDK.Repository;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProcessImage.Entities;
using ProcessImage.Helpers;
using ProcessImage.Services;
using ProcessImage.Services.Interface;
using System.Linq.Expressions;
namespace ProcessImage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DashBoardController : Controller
    {
        private readonly IBaseService _baseService;
        private readonly IDashBoardService _dashBoardService;
        private readonly ICacheService _cacheService;


        private const string CACHE_KEY_STATS = "dashboard_stats_{0}"; 
        private const string CACHE_KEY_PROCESSING = "dashboard_processing_{0}_{1}_{2}"; 
        private const string CACHE_KEY_FILES = "dashboard_files_{0}_{1}_{2}";
        private const string CACHE_PATTERN_STATS = "dashboard_stats_";
        private const string CACHE_PATTERN_PROCESSING = "dashboard_processing_";
        private const string CACHE_PATTERN_FILES = "dashboard_files_";

        public DashBoardController(
            IBaseService baseService,
            IDashBoardService dashBoardService,
            ICacheService cacheService)
        {
            _baseService = baseService;
            _dashBoardService = dashBoardService;
            _cacheService = cacheService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var userId = _baseService.GetUserId();
                string cacheKey = string.Format(CACHE_KEY_STATS, userId);

                if (_cacheService.Exists(cacheKey))
                {
                    var cachedStats = _cacheService.Get<IActionResult>(cacheKey);
                    return cachedStats;
                }

                var result = await _dashBoardService.GetStatsAsync(userId);

                _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(10));

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la preluarea statisticilor.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("processing")]
        public async Task<IActionResult> GetProcessingData([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                var userId = _baseService.GetUserId();
                string cacheKey = string.Format(CACHE_KEY_PROCESSING, userId, skip, take);

                if (_cacheService.Exists(cacheKey))
                {
                    var cachedData = _cacheService.Get<IActionResult>(cacheKey);
                    return cachedData;
                }
                var result = await _dashBoardService.GetProcessingDataAsync(userId, skip, take);

                _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(15));

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la preluarea datelor de procesare.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("files")]
        public async Task<IActionResult> GetFilesData([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                var userId = _baseService.GetUserId();
                string cacheKey = string.Format(CACHE_KEY_FILES, userId, skip, take);

                if (_cacheService.Exists(cacheKey))
                {
                    var cachedData = _cacheService.Get<IActionResult>(cacheKey);
                    return cachedData;
                }

                var result = await _dashBoardService.GetFilesDataAsync(userId, skip, take);

                _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(15));

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la preluarea datelor de fisiere.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("deleteProcessingData/{id}")]
        public async Task<IActionResult> DeleteProcessingData(int id)
        {
            try
            {
                var userId = _baseService.GetUserId();
                var result = await _dashBoardService.DeleteProcessingDataAsync(id, userId);

                _cacheService.RemoveByPattern($"dashboard_processing_{userId}");
                _cacheService.Remove(string.Format(CACHE_KEY_STATS, userId));

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la stergerea datelor de procesare.",
                    error = ex.Message
                });
            }
        }


        [HttpDelete("deleteFile/{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var userId = _baseService.GetUserId();
                var result = await _dashBoardService.DeleteFileAsync(id, userId);

                _cacheService.RemoveByPattern($"dashboard_files_{userId}");
                _cacheService.Remove(string.Format(CACHE_KEY_STATS, userId));

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Eroare la stergerea fisierului",
                    error = ex.Message
                });
            }
        }

        [HttpGet("download/{fileName}")]
        public IActionResult DownloadImage(string fileName)
        {
            try
            {
                var userId = _baseService.GetUserId();
                string basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "imagini");
                string userFolderPath = Path.Combine(basePath, userId.ToString());
                string fullPath = Path.Combine(userFolderPath, fileName);

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new { message = "Fișierul nu a fost găsit" });
                }

                var fileBytes = System.IO.File.ReadAllBytes(fullPath);
                var contentType = "image/png"; 

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Eroare la descărcarea fișierului", error = ex.Message });
            }
        }
    }
}
