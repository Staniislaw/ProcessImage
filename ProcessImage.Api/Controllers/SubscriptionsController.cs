using Data.SDK.Repository;
using ProcessImage.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ProcessImage.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ProcessImage.Models.DTO;
using ProcessImage.Services;

namespace ProcessImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SubscriptionsController : ControllerBase
    {
        private readonly IRepository<Subscriptie> _subscriptieRepository;
        private readonly IRepository<Utilizator> _utilizatorRepository;
        private readonly IRepository<TipProcesare> _tipProcesareRepository;
        private readonly IBaseService _baseService;
        private readonly ISubscriptionsService _subscriptionsService;
        private readonly ICacheService _cacheService;


        private const string CACHE_KEY_ALL_SUBSCRIPTIONS = "all_subscriptions";
        private const string CACHE_KEY_SUBSCRIPTION_ID = "subscription_{0}"; 
        private const string CACHE_KEY_SUBSCRIPTION_TYPE = "subscription_type_{0}";
        private const string CACHE_KEY_ALL_PROCESSING_TYPES = "all_processing_types";
        private const string CACHE_KEY_SUBSCRIPTION_LIMITS = "subscription_limits_{0}";
        private const string CACHE_KEY_SUBSCRIPTIONS_REPORT = "subscriptions_report";

        private const string CACHE_PATTERN_SUBSCRIPTIONS = "subscription_";
        private const string CACHE_PATTERN_PROCESSING_TYPES = "processing_";

        public SubscriptionsController(
            IRepository<Subscriptie> subscriptieRepository,
            IRepository<TipProcesare> tipProcesareRepository,
            IRepository<Utilizator> utilizatorRepository,
            ISubscriptionsService subscriptionsService,
            ICacheService cacheService,
            IBaseService baseService)
        {
            _subscriptieRepository = subscriptieRepository;
            _tipProcesareRepository = tipProcesareRepository;
            _utilizatorRepository = utilizatorRepository;
            _baseService = baseService;
            _cacheService = cacheService;
            _subscriptionsService = subscriptionsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subscriptie>>> GetAllSubscriptions()
        {
            try
            {
                if (_cacheService.Exists(CACHE_KEY_ALL_SUBSCRIPTIONS))
                {
                    var cachedSubscriptions = _cacheService.Get<IEnumerable<Subscriptie>>(CACHE_KEY_ALL_SUBSCRIPTIONS);
                    return Ok(cachedSubscriptions);
                }

                var subscriptions = await _subscriptieRepository.GetAllAsync(
                    includes: query => query
                        .Include(s => s.SubscripteProcesares)
                            .ThenInclude(sp => sp.TipProcesare)
                );

                if (subscriptions == null || !subscriptions.Any())
                {
                    return NotFound(new { message = "Nu au fost găsite abonamente." });
                }
                _cacheService.Set(CACHE_KEY_ALL_SUBSCRIPTIONS, subscriptions, TimeSpan.FromMinutes(30));

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
                string cacheKey = string.Format(CACHE_KEY_SUBSCRIPTION_ID, id);
                if (_cacheService.Exists(cacheKey))
                {
                    var cachedSubscription = _cacheService.Get<Subscriptie>(cacheKey);
                    return Ok(cachedSubscription);
                }

                var subscription = await _subscriptieRepository.GetAsync(
                    predicate: s => s.Id == id,
                    includes: query => query
                        .Include(s => s.SubscripteProcesares)
                            .ThenInclude(sp => sp.TipProcesare)
                );

                if (subscription == null)
                {
                    return NotFound(new { message = $"Abonamentul cu ID-ul {id} nu a fost gasit." });
                }

                _cacheService.Set(cacheKey, subscription, TimeSpan.FromMinutes(30));

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
                string cacheKey = string.Format(CACHE_KEY_SUBSCRIPTION_TYPE, tip.ToLower());

                if (_cacheService.Exists(cacheKey))
                {
                    var cachedSubscription = _cacheService.Get<Subscriptie>(cacheKey);
                    return Ok(cachedSubscription);
                }

                var subscription = await _subscriptieRepository.GetAsync(
                    predicate: s => s.Tip.ToLower() == tip.ToLower(),
                    includes: query => query
                        .Include(s => s.SubscripteProcesares)
                            .ThenInclude(sp => sp.TipProcesare)
                );

                if (subscription == null)
                {
                    return NotFound(new { message = $"Abonamentul de tip '{tip}' nu a fost gasit." });
                }
                _cacheService.Set(cacheKey, subscription, TimeSpan.FromMinutes(30));

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
                if (_cacheService.Exists(CACHE_KEY_ALL_PROCESSING_TYPES))
                {
                    var cachedTypes = _cacheService.Get<IEnumerable<TipProcesare>>(CACHE_KEY_ALL_PROCESSING_TYPES);

                    return Ok(cachedTypes);
                }
                var processingTypes = await _tipProcesareRepository.GetAllAsync();

                if (processingTypes == null || !processingTypes.Any())
                {
                    return NotFound(new { message = "Nu au fost gasite tipuri de procesare." });
                }

                _cacheService.Set(CACHE_KEY_ALL_PROCESSING_TYPES, processingTypes, TimeSpan.FromHours(1));

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
                string cacheKey = string.Format(CACHE_KEY_SUBSCRIPTION_LIMITS, id);

                if (_cacheService.Exists(cacheKey))
                {
                    var cachedLimits = _cacheService.Get<dynamic>(cacheKey);
                    return Ok(cachedLimits);
                }

                var response = await _subscriptionsService.GetSubscriptionLimitsAsync(id);

                _cacheService.Set(cacheKey, response, TimeSpan.FromMinutes(15));

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
                if (_cacheService.Exists(CACHE_KEY_SUBSCRIPTIONS_REPORT))
                {
                    var cachedReport = _cacheService.Get<dynamic>(CACHE_KEY_SUBSCRIPTIONS_REPORT);
                    return Ok(cachedReport);
                }

                var report = await _subscriptionsService.GetSubscriptionsReportAsync();

                _cacheService.Set(CACHE_KEY_SUBSCRIPTIONS_REPORT, report, TimeSpan.FromHours(1));

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
        public async Task<IActionResult> ActivateSubscription(int id, [FromQuery] int? utilizatorId)
        {
            try
            {
                if (!utilizatorId.HasValue)
                {
                    utilizatorId = _baseService.GetUserId();
                }

                var utilizator = await _utilizatorRepository.GetAsync(u => u.Id == utilizatorId);
                if (utilizator == null)
                    return NotFound(new { message = "Utilizator nu gasit" });

                var subscriptie = await _subscriptieRepository.GetAsync(s => s.Id == id);
                if (subscriptie == null)
                    return NotFound(new { message = "Abonamentul selectat nu exista" });

                utilizator.SubscriptieId = subscriptie.Id;
                await _utilizatorRepository.UpdateAsync(utilizator);
                await _utilizatorRepository.SaveChangesAsync();

                _cacheService.Remove(string.Format(CACHE_KEY_SUBSCRIPTION_ID, id));
                _cacheService.Remove(string.Format(CACHE_KEY_SUBSCRIPTION_TYPE, subscriptie.Tip.ToLower()));
                _cacheService.Remove(CACHE_KEY_ALL_SUBSCRIPTIONS);
                _cacheService.Remove(CACHE_KEY_SUBSCRIPTIONS_REPORT);

                return Ok(new
                {
                    message = $"Abonamentul {subscriptie.Tip} a fost activat cu succes!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la activarea abonamentului.",
                    error = ex.Message
                });
            }
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