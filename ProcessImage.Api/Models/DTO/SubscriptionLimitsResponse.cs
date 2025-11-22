namespace ProcessImage.Models.DTO
{
    public class LimitDto
    {
        public long TipProcesareId { get; set; }
        public string TipProcesare { get; set; } = string.Empty;
        public long? LimitaMax { get; set; }
        public bool EsteLimitat { get; set; }
        public string Descriere { get; set; } = string.Empty;
    }
    public class SubscriptionLimitsResponse
    {
        public long SubscriptieId { get; set; }
        public string Tip { get; set; } = string.Empty;
        public List<LimitDto> Limite { get; set; } = new();
    }

}
