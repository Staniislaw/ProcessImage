using Data.SDK.Repository;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProcessImage.Entities;
using ProcessImage.Helpers;
using ProcessImage.Services.Interface;
using System.Linq.Expressions;
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
        public async Task<IActionResult> GetProcessingData([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var userId = _baseService.GetUserId();

            var allProcessing = await _procesareImaginiRepository.GetWhereIncludeAsync(
                p => p.Imagine.UtilizatorId == userId,
                false,
                new Expression<Func<ProcesareImagine, object>>[]
                {
            x => x.Imagine,
            x => x.TipProcesare
                }
            );
            var total = allProcessing.Count();
            var pagedProcessing = allProcessing
                .Skip(skip)
                .Take(take)
                .Select(p => new
                {
                    p.Id,
                    p.ImagineId,
                    TipProcesare = p.TipProcesare.Nume,
                    p.Status,
                    p.DataProcesare
                })
                .ToList();

            return Ok(new
            {
                processing = pagedProcessing,
                total
            });
        }


        [HttpGet("files")]
        public async Task<IActionResult> GetFilesData([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                var userId = _baseService.GetUserId();
                var query = await _imagineRepository.FindAsync(f => f.UtilizatorId == userId);
                var total = query.Count();
                var files = query
                            .Skip(skip)
                            .Take(take)
                            .Select(f => new
                            {
                                f.Id,
                                f.Nume,
                                f.Tip,
                                f.UtilizatorId,
                                f.DataIncarcarii
                            })
                            .ToList();
                return Ok(new
                {
                    files = files,
                    total = total
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Eroare la preluarea fișierelor", error = ex.Message });
            }
        }
        [HttpDelete("deleteProcessingData/{id}")]
        public async Task<IActionResult> DeleteProcessingData(int id)
        {
            var userId = _baseService.GetUserId();
            var logger = LoggerAppender.GetLoggerForApplicationType(
                LoggerAppender.LogApplication.Utilizatori,
                userId
            );

            logger.Info($"Userul {userId} a acționat butonul de ștergere date procesate");

            try
            {
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Nu aveți permisiune să ștergeți aceste date" });
                }

                var procesareImagine = await _procesareImaginiRepository.GetAsync(x => x.Id == id);
                if (procesareImagine == null)
                {
                    return NotFound(new { message = "Datele procesate nu au fost găsite" });
                }

                var imagine = await _imagineRepository.GetAsync(x => x.Id == procesareImagine.ImagineId);
                if (imagine == null)
                {
                    return NotFound(new { message = "Imaginea asociată nu a fost găsită" });
                }

                await _procesareImaginiRepository.DeleteAsync(procesareImagine);
                await _procesareImaginiRepository.SaveChangesAsync();

                logger.Info($"Datele procesate cu id {id} au fost șterse cu succes.");
                return Ok(new { message = "Datele procesate au fost șterse cu succes" });
            }
            catch (Exception ex)
            {
                logger.Error($"Eroare la ștergerea datelor procesate cu id {id}: {ex}", ex);
                return BadRequest(new { message = "Eroare la ștergerea datelor procesate", error = ex.Message });
            }
        }


        [HttpDelete("deleteFile/{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var userId = _baseService.GetUserId();
            var logger = LoggerAppender.GetLoggerForApplicationType(
                   LoggerAppender.LogApplication.Utilizatori,
                   userId
               );
            try
            {
                var imagine = await _imagineRepository.GetAsync(x => x.Id == id);
                logger.Info($"Userul {userId} a acționat butonul de stergere fisiere");

                if (imagine == null)
                {
                    return NotFound(new { message = "Fiыierul nu a fost gasit" });
                }
                if (imagine.UtilizatorId != userId)
                {
                    return Unauthorized(new { message = "Nu aveеi permisiune sф ыtergeеi acest fiыier" });
                }
                var procesariAsociate = await _procesareImaginiRepository.FindAsync(p => p.ImagineId == id);
                foreach (var procesare in procesariAsociate)
                {
                    await _procesareImaginiRepository.DeleteAsync(procesare);
                }
                await _imagineRepository.DeleteAsync(imagine);
                await _imagineRepository.SaveChangesAsync();

                return Ok(new { message = "Fiыierul ыi toate datele procesate asociate au fost ыterse cu succes" });
            }
            catch (Exception ex)
            {
                logger.Error($"Eroare la ștergerea datelor procesate cu id {id}: {ex}", ex);
                return BadRequest(new { message = "Eroare la ыtergerea fiierului", error = ex.Message });
            }
        }
    }
}
