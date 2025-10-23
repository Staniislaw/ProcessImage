using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProcessImage.Entities;
using ProcessImage.Repository.Interfaces;

namespace ProcessImage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilizatorController : ControllerBase
    {
        private readonly IUtilizatorRepository _utilizatorRepository;
        private readonly IConfiguration _configuration;

        public UtilizatorController(IUtilizatorRepository utilizatorRepository, IConfiguration configuration)
        {
            _utilizatorRepository = utilizatorRepository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verifică dacă utilizatorul există
            if (await _utilizatorRepository.ExistsAsync(request.Email))
                return BadRequest(new { message = "Utilizatorul cu acest email există deja" });

            var parolaHashata = BCrypt.Net.BCrypt.HashPassword(request.Parola);

            var utilizator = new Utilizator
            {
                Nume = request.Nume,
                Email = request.Email,
                Parola = parolaHashata,
                SubscriptieId = request.SubscriptieId
            };

            var result = await _utilizatorRepository.CreateAsync(utilizator);

            return Ok(new { message = "Utilizator înregistrat cu succes", utilizatorId = result.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var utilizator = await _utilizatorRepository.GetByEmailAsync(request.Email);
            if (utilizator == null)
                return Unauthorized(new { message = "Email sau parolă incorectă" });
            var isValid = BCrypt.Net.BCrypt.Verify(request.Parola, utilizator.Parola);
            Console.WriteLine($"Verify rezultat: {isValid}");

            if (!BCrypt.Net.BCrypt.Verify(request.Parola, utilizator.Parola))
                return Unauthorized(new { message = "Email sau parolă incorectă" });

            var token = GenerateJwtToken(utilizator);

            return Ok(new
            {
                message = "Logare reușită",
                token = token,
                utilizator = new
                {
                    id = utilizator.Id,
                    nume = utilizator.Nume,
                    email = utilizator.Email
                }
            });
        }

        [HttpGet("profil")]
        [Authorize]
        public async Task<IActionResult> GetProfil()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Token invalid" });

            var utilizator = await _utilizatorRepository.GetByIdAsync(userId);
            if (utilizator == null)
                return NotFound(new { message = "Utilizator nu găsit" });

            return Ok(new
            {
                id = utilizator.Id,
                nume = utilizator.Nume,
                email = utilizator.Email
            });
        }

        [HttpPut("update-profil")]
        [Authorize]
        public async Task<IActionResult> UpdateProfil([FromBody] UpdateProfilRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Token invalid" });

            var utilizator = await _utilizatorRepository.GetByIdAsync(userId);
            if (utilizator == null)
                return NotFound(new { message = "Utilizator nu găsit" });

            utilizator.Nume = request.Nume ?? utilizator.Nume;
            await _utilizatorRepository.UpdateAsync(utilizator);

            return Ok(new { message = "Profil actualizat cu succes" });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Token invalid" });

            var utilizator = await _utilizatorRepository.GetByIdAsync(userId);
            if (utilizator == null)
                return NotFound(new { message = "Utilizator nu găsit" });

            if (!BCrypt.Net.BCrypt.Verify(request.ParolaVeche, utilizator.Parola))
                return BadRequest(new { message = "Parola veche incorectă" });

            utilizator.Parola = BCrypt.Net.BCrypt.HashPassword(request.ParolaNoua);
            await _utilizatorRepository.UpdateAsync(utilizator);

            return Ok(new { message = "Parola schimbată cu succes" });
        }

        private string GenerateJwtToken(Utilizator utilizator)
        {
            var key = _configuration["Jwt:Key"] ?? "supersecret_secretkey!123";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilizator.Id.ToString()),
                new Claim(ClaimTypes.Email, utilizator.Email),
                new Claim(ClaimTypes.Name, utilizator.Nume),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterRequest
    {
        public string Nume { get; set; }
        public string Email { get; set; }
        public string Parola { get; set; }
        public long SubscriptieId { get; set; } 
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Parola { get; set; }
    }

    public class UpdateProfilRequest
    {
        public string Nume { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string ParolaVeche { get; set; }
        public string ParolaNoua { get; set; }
    }
}
