namespace ProcessImage.Entities
{
    public class Imagine
    {
        public long Id { get; set; }
        public string Nume { get; set; }
        public string Tip { get; set; }
        public long UtilizatorId { get; set; }
        public DateTime DataIncarcarii { get; set; }
        public string CaleFisier { get; set; }
        public virtual Utilizator Utilizator { get; set; }
    }
}
