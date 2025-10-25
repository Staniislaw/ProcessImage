using ProcessImage.Services.Interface;

using System.Security.Claims;

namespace ProcessImage.Services
{
    public class BaseService :IBaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public int GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("Utilizator neautentificat");

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("Token invalid");

            return userId;
        }
        public int? GetUserIdOrNull()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;

            return null;
        }
        public string GetUserEmail()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("Utilizator neautentificat");

            return user.FindFirst(ClaimTypes.Email)?.Value
                ?? throw new UnauthorizedAccessException("Email lipsă din token");
        }
        public int GetClaimAsInteger(string claimType)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("Utilizator neautentificat");

            var claim = user.FindFirst(claimType);
            if (claim == null || !int.TryParse(claim.Value, out int value))
                throw new UnauthorizedAccessException($"Claim {claimType} invalid sau lipsă");

            return value;
        }

    }
}
