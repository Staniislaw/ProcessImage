namespace ProcessImage.Entities
{
    public class ProcesareImagine
    {
        public long Id { get; set; }
        public long ImagineId { get; set; }
        public long TipProcesareId { get; set; }
        public string Status { get; set; }
        public DateTime DataProcesare { get; set; }
        public virtual Imagine Imagine { get; set; }
        public virtual TipProcesare TipProcesare { get; set; }
    }
}