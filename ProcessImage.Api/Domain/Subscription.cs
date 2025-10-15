using Base.SDK;

namespace ProcessImage.Domain
{
    public class Subscription : BaseEntity
    {
        public string? Tip { get; set; }
        public decimal Pret { get; set; }
        public int DimensiuneMaximaMb { get; set; }
        public int? SubscriptieProcesareID { get; set; }
    }
}
