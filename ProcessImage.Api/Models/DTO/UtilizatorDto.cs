namespace ProcessImage.Models.DTO
{
    public class UtilizatorDto
    {
        public long Id { get; set; }
        public string Nume { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public UtilizatorDto Utilizator { get; set; } = new();
    }
}
