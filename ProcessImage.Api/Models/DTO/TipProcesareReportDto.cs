namespace ProcessImage.Models.DTO
{
    public class TipProcesareReportDto
    {
        public long Id { get; set; }
        public string Nume { get; set; } = string.Empty;
        public long TipProcesareId { get; set; }
        public int? LimitaMax { get; set; }
        public bool EsteLimitat { get; set; }
        public string Status { get; set; } = string.Empty;
    }

}
