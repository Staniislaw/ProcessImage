using Data.SDK.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ProcessImage.Entities;
using ProcessImage.Models.Enum;
using ProcessImage.Services.Interface;

using System.Security.Claims;

namespace ProcessImage.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Utilizator> _utilizatorRepository;
        private readonly IRepository<SubscripteProcesare> _subscriptionProcRepository;
        private readonly IRepository<ProcesareImagine> _procesareRepository;

        public BaseService(
            IHttpContextAccessor httpContextAccessor,
            IRepository<Utilizator> utilizatorRepository = null,
            IRepository<SubscripteProcesare> subscriptionProcRepository = null,
            IRepository<ProcesareImagine> procesareRepository = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _utilizatorRepository = utilizatorRepository;
            _subscriptionProcRepository = subscriptionProcRepository;
            _procesareRepository = procesareRepository;
        }

        public int GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("Utilizator neautentificat");
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("Token invalid");
            return userId;
        }

        public int? GetUserIdOrNull()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;
            return null;
        }

        public string GetUserEmail()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("Utilizator neautentificat");
            return user.FindFirst(ClaimTypes.Email)?.Value
                ?? throw new UnauthorizedAccessException("Email lipsă din token");
        }

        public int GetClaimAsInteger(string claimType)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("Utilizator neautentificat");
            var claim = user.FindFirst(claimType);
            if (claim == null || !int.TryParse(claim.Value, out int value))
                throw new UnauthorizedAccessException($"Claim {claimType} invalid sau lipsă");
            return value;
        }

        public async Task<(bool CanProcess, string Message, int Used, int Remaining)> CheckProcessingLimitAsync(int userId, long processingTypeId)
        {
            if (_utilizatorRepository == null || _subscriptionProcRepository == null || _procesareRepository == null)
                throw new InvalidOperationException("Repositories nu sunt inițializate în BaseService");
            try
            {
                var user = await _utilizatorRepository.GetAsync(
                    u => u.Id == userId,
                    includes: q => q.Include(u => u.Subscriptie)
                );
                if (user?.Subscriptie == null)
                    return (false, "Fara subscriptie", 0, 0);
                var subProc = await _subscriptionProcRepository.GetAsync(
                    sp => sp.SubscriptieId == user.SubscriptieId && sp.TipProcesareId == processingTypeId
                );
                if (subProc == null)
                    return (false, $"Tipul de procesare #{processingTypeId} nu e disponibil pentru subscripTia {user.Subscriptie.Tip}", 0, 0);
                if (subProc.LimitaMax == null)
                    return (true, "Acces nelimitat", 0, -1);
                var now = DateTime.Now;
                var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                var endOfDay = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, 999);
                var allProcesari = await _procesareRepository.GetAllAsync(
                    includes: q => q.Include(p => p.Imagine)
                );
                var count = allProcesari.Count(
                    p => p.Imagine.UtilizatorId == userId &&
                         p.TipProcesareId == processingTypeId &&
                         p.DataProcesare >= startOfDay &&
                         p.DataProcesare < endOfDay &&
                         p.Status == ProcessingStatusEnum.Success
                );
                var remaining = subProc.LimitaMax.Value - count;
                if (count >= subProc.LimitaMax.Value)
                    return (false, $"Limita atinsa: {count}/{subProc.LimitaMax} procesAri luna aceasta", count, 0);
                return (true, $"{count}/{subProc.LimitaMax} utilizAri luna aceastA", count, remaining);
            }
            catch (Exception ex)
            {
                return (false, $"eroare: {ex.Message}", 0, 0);
            }
        }
        public async Task LogProcessingAsync(int imagineId, long processingTypeId, string status)
        {
            if (_procesareRepository == null)
                throw new InvalidOperationException("ProcesareRepository nu este inițializat în BaseService");

            try
            {
                var record = new ProcesareImagine
                {
                    ImagineId = imagineId,
                    TipProcesareId = processingTypeId,
                    Status = status,
                    DataProcesare = DateTime.UtcNow
                };

                await _procesareRepository.AddAsync(record);
                await _procesareRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la logging procesare: {ex.Message}");
            }
        }
        public async Task<IActionResult> ValidateProcessingLimitAsync(int userId, long processingTypeId, Func<Task<IActionResult>> processAction)
        {
            var (canProcess, message, used, remaining) = await CheckProcessingLimitAsync(userId, processingTypeId);

            if (!canProcess)
            {
                return new ObjectResult(new
                {
                    canProcess,
                    message,
                    used,
                    remaining
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
            return await processAction();
        }
    }

}
