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
        public DashBoardController(
            IBaseService baseService,
            IDashBoardService dashBoardService)
        {
            _baseService = baseService;
            _dashBoardService = dashBoardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userId = _baseService.GetUserId();
            return await _dashBoardService.GetStatsAsync(userId);
        }

        [HttpGet("processing")]
        public async Task<IActionResult> GetProcessingData([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var userId = _baseService.GetUserId();
            return await _dashBoardService.GetProcessingDataAsync(userId, skip, take);
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetFilesData([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var userId = _baseService.GetUserId();
            return await _dashBoardService.GetFilesDataAsync(userId, skip, take);
        }

        [HttpDelete("deleteProcessingData/{id}")]
        public async Task<IActionResult> DeleteProcessingData(int id)
        {
            var userId = _baseService.GetUserId();
            return await _dashBoardService.DeleteProcessingDataAsync(id, userId);
        }


        [HttpDelete("deleteFile/{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var userId = _baseService.GetUserId();
            try
            {
                return await _dashBoardService.DeleteFileAsync(id, userId);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Eroare la ыtergerea fiierului", error = ex.Message });
            }
        }
    }
}
