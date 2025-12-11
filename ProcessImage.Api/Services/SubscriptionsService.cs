using Data.SDK.Repository;

using Microsoft.EntityFrameworkCore;

using ProcessImage.Entities;
using ProcessImage.Models.DTO;
using ProcessImage.Services.Interface;

namespace ProcessImage.Services
{
    public class SubscriptionsService : ISubscriptionsService
    {
        private readonly IRepository<Subscriptie> _subscriptieRepository;
        public SubscriptionsService(IRepository<Subscriptie> subscriptieRepository)
        {
            _subscriptieRepository = subscriptieRepository;
        }
        public async Task<SubscriptionLimitsResponse> GetSubscriptionLimitsAsync(int id)
        {
            var subscription = await _subscriptieRepository.GetAsync(
                predicate: s => s.Id == id,
                includes: query => query
                    .Include(s => s.SubscripteProcesares)
                        .ThenInclude(sp => sp.TipProcesare)
            );

            if (subscription == null)
            {
                throw new KeyNotFoundException($"Abonamentul cu ID-ul {id} nu a fost gasit.");
            }

            var limits = subscription.SubscripteProcesares.Select(sp => new LimitDto
            {
                TipProcesareId = sp.TipProcesareId,
                TipProcesare = sp.TipProcesare.Nume,
                LimitaMax = sp.LimitaMax,
                EsteLimitat = sp.LimitaMax.HasValue,
                Descriere = sp.LimitaMax.HasValue
                    ? $"Limita de {sp.LimitaMax} procesari pe zi"
                    : "Nelimitat"
            }).ToList();

            return new SubscriptionLimitsResponse
            {
                SubscriptieId = subscription.Id,
                Tip = subscription.Tip,
                Limite = limits
            };
        }
        public async Task<List<SubscriptionReportDto>> GetSubscriptionsReportAsync()
        {
            var subscriptions = await _subscriptieRepository.GetAllAsync(
                includes: query => query
                    .Include(s => s.SubscripteProcesares)
                        .ThenInclude(sp => sp.TipProcesare)
            );
            var report = subscriptions.Select(s => new SubscriptionReportDto
            {
                Id = s.Id,
                Tip = s.Tip,
                Pret = s.Pret,
                DimensiuneMaximaMb = s.DimensiuneMaximaMb,
                SubscriptieProcesareId = s.SubscriptieProcesareId,
                NumarTipuriProcesare = s.SubscripteProcesares.Count,
                TipuriProcesare = s.SubscripteProcesares.Select(sp => new TipProcesareReportDto
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
            })
            .OrderBy(s => s.Pret)
            .ToList();

            return report;
        }

    }
}
