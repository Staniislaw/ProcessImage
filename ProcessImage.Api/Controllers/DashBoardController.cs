using Data.SDK.Repository;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProcessImage.Entities;
using ProcessImage.Services.Interface;

using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ProcessImage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DashBoardController : Controller
    {
        private readonly IRepository<ProcesareImagine> _procesareImaginiRepository;
        private readonly IRepository<Imagine> _imagineRepository;
        private readonly IRepository<TipProcesare> _tipProcesareRepository;
        private readonly IBaseService _baseService;

        public DashBoardController(
            IRepository<ProcesareImagine> procesareImaginiRepository,
            IRepository<Imagine> imagineRepository,
            IRepository<TipProcesare> tipProcesareRepository,
            IBaseService baseService)
        {
            _procesareImaginiRepository = procesareImaginiRepository;
            _imagineRepository = imagineRepository;
            _tipProcesareRepository = tipProcesareRepository;
            _baseService = baseService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userId = _baseService.GetUserId();

            var totalFiles = (await _imagineRepository.FindAsync(f => f.UtilizatorId == userId)).Count();

            var allProcessing = await _procesareImaginiRepository.GetWhereIncludeAsync(
                p => p.Imagine.UtilizatorId == userId,
                false,
                new Expression<Func<ProcesareImagine, object>>[] {
                    x => x.Imagine,
                    x => x.TipProcesare
                }
            );

            var processed = allProcessing.Count(p => p.Status == "Success");

            var successRate = totalFiles == 0 ? "0%" : $"{(processed * 100 / totalFiles)}%";

            var tipProcesarePreferat = allProcessing
                .GroupBy(p => p.TipProcesare.Nume)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            var stats = new object[]
            {
                new { label = "Total Fișiere", value = (object)totalFiles, icon = "upload", color = "blue" },
                new { label = "Procesate", value = (object)processed, icon = "bolt", color = "green" },
                new { label = "Rata Succes", value = (object)successRate, icon = "check_circle", color = "purple" },
                new { label = "Tip Procesare", value = (object)tipProcesarePreferat, icon = "trending_up", color = "orange" }
            };

            return Ok(stats);
        }

        [HttpGet("processing")]
        public async Task<IActionResult> GetProcessingData()
        {
            var userId = _baseService.GetUserId();

            var processing = (await _procesareImaginiRepository.GetWhereIncludeAsync(
                p => p.Imagine.UtilizatorId == userId,
                false,
                new Expression<Func<ProcesareImagine, object>>[]
                {
            x => x.Imagine,
            x => x.TipProcesare
                }
            ))
            .Select(p => new
            {
                p.Id,
                p.ImagineId,
                TipProcesare = p.TipProcesare.Nume,
                p.Status,
                p.DataProcesare
            })
            .ToList();

            return Ok(processing);
        }


        [HttpGet("files")]
        public async Task<IActionResult> GetFilesData()
        {
            var userId = _baseService.GetUserId();

            var files = (await _imagineRepository.FindAsync(f => f.UtilizatorId == userId))
                        .Select(f => new
                        {
                            f.Id,
                            f.Nume,
                            f.Tip,
                            f.UtilizatorId,
                            f.DataIncarcarii
                        })
                        .ToList();

            return Ok(files);
        }
    }
}
