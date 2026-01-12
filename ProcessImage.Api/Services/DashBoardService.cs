using Data.SDK.Repository;

using log4net.Core;

using Microsoft.AspNetCore.Mvc;

using ProcessImage.Entities;
using ProcessImage.Helpers;
using ProcessImage.Services.Interface;

using System.Linq.Expressions;

namespace ProcessImage.Services
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IRepository<ProcesareImagine> _procesareImaginiRepository;
        private readonly IRepository<Imagine> _imagineRepository;
        private readonly IRepository<TipProcesare> _tipProcesareRepository;
        private readonly IBaseService _baseService;

        public DashBoardService(
            IRepository<Imagine> imagineRepository,
            IRepository<ProcesareImagine> procesareImaginiRepository,
            IRepository<TipProcesare> tipProcesareRepository,
            IBaseService baseService)
        {
            _imagineRepository = imagineRepository;
            _procesareImaginiRepository = procesareImaginiRepository;
            _tipProcesareRepository = tipProcesareRepository;
            _baseService = baseService;
        }
        public async Task<IActionResult> DeleteFileAsync(int id, int userId)
        {
            var logger = LoggerAppender.GetLoggerForApplicationType(LoggerAppender.LogApplication.Utilizatori, userId );
            try
            {
                var imagine = await _imagineRepository.GetAsync(x => x.Id == id);
                logger.Info($"Userul {userId} a acționat butonul de ștergere fisiere");

                if (imagine == null)
                    return new NotFoundObjectResult(new { message = "Fișierul nu a fost găsit" });

                if (imagine.UtilizatorId != userId)
                    return new UnauthorizedObjectResult(new { message = "Nu aveți permisiune să ștergeți acest fișier" });

                var procesariAsociate = await _procesareImaginiRepository.FindAsync(p => p.ImagineId == id);

                foreach (var procesare in procesariAsociate)
                    await _procesareImaginiRepository.DeleteAsync(procesare);

                await _imagineRepository.DeleteAsync(imagine);
                await _imagineRepository.SaveChangesAsync();

                return new OkObjectResult(new { message = "Fișierul și toate datele procesate asociate au fost șterse cu succes" });
            }
            catch (Exception ex)
            {
                logger.Error($"Eroare la ștergerea datelor procesate cu id {id}: {ex}", ex);

                return new BadRequestObjectResult(new
                {
                    message = "Eroare la ștergerea fișierului",
                    error = ex.Message
                });
            }
        }
        public async Task<IActionResult> DeleteProcessingDataAsync(int id, int userId)
        {
            var logger = LoggerAppender.GetLoggerForApplicationType(
                LoggerAppender.LogApplication.Utilizatori,
                userId
            );
            logger.Info($"Userul {userId} a actionat butonul de stergere date procesate");
            try
            {
                if (userId == 0)
                {
                    return new UnauthorizedObjectResult(new
                    {
                        message = "Nu aveti permisiune să stergeti aceste date"
                    });
                }
                var procesareImagine = await _procesareImaginiRepository.GetAsync(x => x.Id == id);
                if (procesareImagine == null)
                {
                    return new NotFoundObjectResult(new
                    {
                        message = "Datele procesate nu au fost gasite"
                    });
                }
                var imagine = await _imagineRepository.GetAsync(x => x.Id == procesareImagine.ImagineId);
                if (imagine == null)
                {
                    return new NotFoundObjectResult(new
                    {
                        message = "Imaginea asociata nu a fost gasită"
                    });
                }
                if (imagine.UtilizatorId != userId)
                {
                    return new UnauthorizedObjectResult(new
                    {
                        message = "Nu aveti permisiune sa stergeti aceste date procesate"
                    });
                }
                await _procesareImaginiRepository.DeleteAsync(procesareImagine);
                await _procesareImaginiRepository.SaveChangesAsync();

                logger.Info($"Datele procesate cu id {id} au fost sterse cu succes.");

                return new OkObjectResult(new
                {
                    message = "Datele procesate au fost sterse cu succes"
                });
            }
            catch (Exception ex)
            {
                logger.Error($"Eroare la stergerea datelor procesate cu id {id}: {ex}", ex);

                return new BadRequestObjectResult(new
                {
                    message = "Eroare la stergerea datelor procesate",
                    error = ex.Message
                });
            }
        }
        public async Task<IActionResult> GetFilesDataAsync(int userId, int skip, int take)
        {
            try
            {
                if (userId == 0)
                {
                    return new UnauthorizedObjectResult(new
                    {
                        message = "Utilizator neautorizat"
                    });
                }
                var filesQuery = await _imagineRepository.FindAsync(f => f.UtilizatorId == userId);
                var total = filesQuery.Count();
                var files = filesQuery
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
                return new OkObjectResult(new
                {
                    files = files,
                    total = total
                });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Eroare la preluarea fișierelor",
                    error = ex.Message
                });
            }
        }
        public async Task<IActionResult> GetProcessingDataAsync(int userId, int skip, int take)
        {
            try
            {
                if (userId == 0)
                {
                    return new UnauthorizedObjectResult(new
                    {
                        message = "Utilizator neautorizat"
                    });
                }
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
                    .OrderByDescending(p => p.DataProcesare)
                    .Skip(skip)
                    .Take(take)
                    .Select(p => new
                    {
                        p.Id,
                        p.ImagineId,
                        TipProcesare = p.TipProcesare.Nume,
                        Status = p.Status.ToString(),
                        DataProcesare = p.DataProcesare.ToString("dd/MM/yyyy HH:mm"),
                        CaleFisier = p.Imagine.CaleFisier, 
                        NumeImagine = p.Imagine.Nume + p.Imagine.Tip
                    })
                    .ToList();

                return new OkObjectResult(new
                {
                    processing = pagedProcessing,
                    total = total
                });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Eroare la preluarea datelor procesate",
                    error = ex.Message
                });
            }
        }
        public async Task<IActionResult> GetStatsAsync(int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return new UnauthorizedObjectResult(new
                    {
                        message = "Utilizator neautorizat"
                    });
                }

                var totalFiles = (await _imagineRepository
                    .FindAsync(f => f.UtilizatorId == userId))
                    .Count();

                var allProcessing = await _procesareImaginiRepository.GetWhereIncludeAsync(
                    p => p.Imagine.UtilizatorId == userId,
                    false,
                    new Expression<Func<ProcesareImagine, object>>[]
                    {
                x => x.Imagine,
                x => x.TipProcesare
                    }
                );

                var processed = allProcessing.Count(p => p.Status == "Success");
                var totalProcessing = allProcessing.Count();
                var successRate = totalProcessing == 0
                    ? "0%"
                    : $"{(processed * 100 / totalProcessing)}%";

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

                return new OkObjectResult(stats);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Eroare la preluarea statisticilor",
                    error = ex.Message
                });
            }
        }

    }
}
