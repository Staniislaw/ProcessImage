namespace ProcessImage.Models.DTO
{
    public class SubscriptionReportDto
    {
        public long Id { get; set; }
        public string Tip { get; set; } = string.Empty;
        public decimal Pret { get; set; }
        public decimal DimensiuneMaximaMb { get; set; }
        public long SubscriptieProcesareId { get; set; }
        public int NumarTipuriProcesare { get; set; }
        public List<TipProcesareReportDto> TipuriProcesare { get; set; } = new();
    }
}
