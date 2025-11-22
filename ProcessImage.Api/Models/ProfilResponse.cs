namespace ProcessImage.Models
{
    public class ProfilResponse
    {
        public long Id { get; set; }
        public string Nume { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long SubscriptieId { get; set; }
    }

}
