namespace ProcessImage.Models
{
    public abstract class AuthRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Parola { get; set; } = string.Empty;
    }
    public class RegisterRequest : AuthRequest
    {
        public string Nume { get; set; } = string.Empty;
        public int SubscriptieId { get; set; }
    }
    public class LoginRequest : AuthRequest
    {

    }
    public class UpdateProfilRequest
    {
        public string Nume { get; set; } = string.Empty;
    }
    public class ChangePasswordRequest
    {
        public string ParolaVeche { get; set; } = string.Empty;
        public string ParolaNoua { get; set; } = string.Empty;
    }
}
