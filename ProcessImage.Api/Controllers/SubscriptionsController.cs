using Data.SDK.Repository;
using ProcessImage.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ProcessImage.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ProcessImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly IRepository<Subscriptie> _subscriptieRepository;
        private readonly IRepository<Utilizator> _utilizatorRepository;
        private readonly IRepository<SubscripteProcesare> _subscriptieProcesareRepository;
        private readonly IRepository<TipProcesare> _tipProcesareRepository;
        private readonly IBaseService _baseService;
        public SubscriptionsController(
            IRepository<Subscriptie> subscriptieRepository,
            IRepository<SubscripteProcesare> subscriptieProcesareRepository,
            IRepository<TipProcesare> tipProcesareRepository,
            IRepository<Utilizator> utilizatorRepository,
            IBaseService baseService)
        {
            _subscriptieRepository = subscriptieRepository;
            _subscriptieProcesareRepository = subscriptieProcesareRepository;
            _tipProcesareRepository = tipProcesareRepository;
            _utilizatorRepository = utilizatorRepository;
            _baseService = baseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subscriptie>>> GetAllSubscriptions()
        {
            try
            {
                var subscriptions = await _subscriptieRepository.GetAllAsync(
                    includes: query => query
                        .Include(s => s.SubscripteProcesares)
                            .ThenInclude(sp => sp.TipProcesare)
                );

                if (subscriptions == null || !subscriptions.Any())
                {
                    return NotFound(new { message = "Nu au fost găsite abonamente." });
                }

                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la preluarea abonamentelor.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Subscriptie>> GetSubscriptionById(int id)
        {
            try
            {
                var subscription = await _subscriptieRepository.GetAsync(
                    predicate: s => s.Id == id,
                    includes: query => query
                        .Include(s => s.SubscripteProcesares)
                            .ThenInclude(sp => sp.TipProcesare)
                );

                if (subscription == null)
                {
                    return NotFound(new { message = $"Abonamentul cu ID-ul {id} nu a fost găsit." });
                }

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la preluarea abonamentului.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("type/{tip}")]
        public async Task<ActionResult<Subscriptie>> GetSubscriptionByType(string tip)
        {
            try
            {
                var subscription = await _subscriptieRepository.GetAsync(
                    predicate: s => s.Tip.ToLower() == tip.ToLower(),
                    includes: query => query
                        .Include(s => s.SubscripteProcesares)
                            .ThenInclude(sp => sp.TipProcesare)
                );

                if (subscription == null)
                {
                    return NotFound(new { message = $"Abonamentul de tip '{tip}' nu a fost găsit." });
                }

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la preluarea abonamentului.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("processing-types")]
        public async Task<ActionResult<IEnumerable<TipProcesare>>> GetAllProcessingTypes()
        {
            try
            {
                var processingTypes = await _tipProcesareRepository.GetAllAsync();

                if (processingTypes == null || !processingTypes.Any())
                {
                    return NotFound(new { message = "Nu au fost găsite tipuri de procesare." });
                }

                return Ok(processingTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la preluarea tipurilor de procesare.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}/limits")]
        public async Task<ActionResult> GetSubscriptionLimits(int id)
        {
            try
            {
                var subscription = await _subscriptieRepository.GetAsync(
                    predicate: s => s.Id == id,
                    includes: query => query
                        .Include(s => s.SubscripteProcesares)
                            .ThenInclude(sp => sp.TipProcesare)
                );

                if (subscription == null)
                {
                    return NotFound(new { message = $"Abonamentul cu ID-ul {id} nu a fost găsit." });
                }

                var limits = subscription.SubscripteProcesares.Select(sp => new
                {
                    TipProcesare = sp.TipProcesare.Nume,
                    TipProcesareId = sp.TipProcesareId,
                    LimitaMax = sp.LimitaMax,
                    EsteLimitat = sp.LimitaMax.HasValue,
                    Descriere = sp.LimitaMax.HasValue
                        ? $"Limită de {sp.LimitaMax} procesări pe lună"
                        : "Nelimitat"
                }).ToList();
                return Ok(new
                {
                    SubscriptieId = subscription.Id,
                    Tip = subscription.Tip,
                    Limite = limits
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la preluarea limitelor.",
                    error = ex.Message
                });
            }
        }
        [HttpGet("report")]
        public async Task<ActionResult> GetSubscriptionsReport()
        {
            try
            {
                var subscriptions = await _subscriptieRepository.GetAllAsync(
                    includes: query => query
                        .Include(s => s.SubscripteProcesares)
                            .ThenInclude(sp => sp.TipProcesare)
                );

                var report = subscriptions.Select(s => new
                {
                    s.Id,
                    s.Tip,
                    s.Pret,
                    DimensiuneMaximaMb = s.DimensiuneMaximaMb,
                    SubscriptieProcesareId = s.SubscriptieProcesareId,
                    NumarTipuriProcesare = s.SubscripteProcesares.Count,
                    TipuriProcesare = s.SubscripteProcesares.Select(sp => new
                    {
                        Id = sp.Id,
                        Nume = sp.TipProcesare.Nume,
                        TipProcesareId = sp.TipProcesareId,
                        LimitaMax = sp.LimitaMax,
                        EsteLimitat = sp.LimitaMax.HasValue,
                        Status = sp.LimitaMax.HasValue
                            ? $"Limitat la {sp.LimitaMax}"
                            : "Nelimitat"
                    }).OrderBy(tp => tp.Nume).ToList()
                }).OrderBy(s => s.Pret).ToList();

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la generarea raportului.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("activate/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ActivateSubscription(int id, [FromQuery] int? utilizatorId)
        {
            if(!utilizatorId.HasValue)
            {
                utilizatorId = _baseService.GetUserId();
            }
            var utilizator = await _utilizatorRepository.GetAsync(u => u.Id == utilizatorId);
            if (utilizator == null)
                return NotFound(new { message = "Utilizator nu găsit" });

            var subscriptie = await _subscriptieRepository.GetAsync(s => s.Id == id);
            if (subscriptie == null)
                return NotFound(new { message = "Abonamentul selectat nu există" });

            utilizator.SubscriptieId = subscriptie.Id;
            await _utilizatorRepository.UpdateAsync(utilizator);
            await _utilizatorRepository.SaveChangesAsync();
            return Ok(new
            {
                message = $"Abonamentul {subscriptie.Tip} a fost activat cu succes!"
            });
        }
        [HttpGet("check-limit")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> CheckProcessingLimit([FromQuery] int tipProcesareId)
        {
            try
            {
                var userId = _baseService.GetUserId();
                var (canProcess, message, used, remaining) = await _baseService.CheckProcessingLimitAsync(userId, tipProcesareId);
                return Ok(new
                {
                    CanProcess = canProcess,
                    Message = message,
                    ProceseUtilizate = used,
                    ProceseRamase = remaining,
                    TipProcesareId = tipProcesareId,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = "Utilizator neautentificat",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la verificarea limitei",
                    error = ex.Message
                });
            }
        }
    }

}