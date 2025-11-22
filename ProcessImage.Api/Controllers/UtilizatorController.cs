using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProcessImage.Entities;
using ProcessImage.Repository.Interfaces;
using Data.SDK.Repository;
using ProcessImage.Services.Interface;
using ProcessImage.Models;
using ProcessImage.Models.Enum;
using ProcessImage.Models.DTO;

namespace ProcessImage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilizatorController : ControllerBase
    {
        private readonly IRepository<Utilizator> _utilizatorRepository;
        private readonly IRepository<Subscriptie> _subscriptieRepository;
        private readonly IConfiguration _configuration;
        private readonly IBaseService _baseService;
        public UtilizatorController(IRepository<Utilizator> utilizatorRepository, IConfiguration configuration, IRepository<Subscriptie> subscriptieRepository, IBaseService baseService)
        {
            _utilizatorRepository = utilizatorRepository;
            _configuration = configuration;
            _subscriptieRepository = subscriptieRepository;
            _baseService = baseService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var utilizatorExistent = await _utilizatorRepository.GetAsync(u => u.Email == request.Email);
            if (utilizatorExistent != null)
                return BadRequest(new { message = "Utilizatorul cu acest email exista deja" });

            var subscriptieFree = await _subscriptieRepository.GetAsync(s => s.Tip == "Free");

            if (subscriptieFree == null)
            {
                return StatusCode(500, new { message = "Subscriptia Free nu este configurata în sistem" });
            }

            var parolaHashata = BCrypt.Net.BCrypt.HashPassword(request.Parola);

            var utilizator = new Utilizator
            {
                Nume = request.Nume,
                Email = request.Email,
                Parola = parolaHashata,
                SubscriptieId = subscriptieFree.Id
            };
            try
            {
                var result = await _utilizatorRepository.AddAsync(utilizator);
                return Ok(new
                {
                    message = "Utilizator inregistrat cu succes",
                    utilizatorId = result.Id,
                    subscriptie = subscriptieFree.Tip
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "A aparut o eroare la inregistrare", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var utilizator = await _utilizatorRepository.GetAsync(u => u.Email == request.Email);
            if (utilizator == null)
                return Unauthorized(new { message = "Email sau parola incorecta" });

            var isValid = BCrypt.Net.BCrypt.Verify(request.Parola, utilizator.Parola);
            if (!isValid)
                return Unauthorized(new { message = "Email sau parola incorecta" });
            var token = GenerateJwtToken(utilizator);
            var response = new LoginResponse
            {
                Message = "Logare reusita",
                Token = token,
                Utilizator = new UtilizatorDto
                {
                    Id = utilizator.Id,
                    Nume = utilizator.Nume,
                    Email = utilizator.Email
                }
            };

            return Ok(response);
        }

        [HttpGet("profil")]
        public async Task<IActionResult> GetProfil()
        {
            var userId = _baseService.GetUserId();

            var utilizator = await _utilizatorRepository.GetAsync(u => u.Id == userId);
            if (utilizator == null)
                return NotFound(new { message = "Utilizator nu gasit" });

            var profil = new ProfilResponse
            {
                Id = utilizator.Id,
                Nume = utilizator.Nume,
                Email = utilizator.Email,
                SubscriptieId = utilizator.SubscriptieId
            };

            return Ok(profil);
        }


        [HttpPut("update-profil")]
        public async Task<IActionResult> UpdateProfil([FromBody] UpdateProfilRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Token invalid" });

            var utilizator = await _utilizatorRepository.GetSingleWhereIncludeAsync(
                    u => u.Id == userId,
                    asNoTracking: true,
                    u => u.Subscriptie);
            if (utilizator == null)
                return NotFound(new { message = "Utilizator nu gasit" });

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
            var utilizator = await _utilizatorRepository.GetSingleWhereIncludeAsync(
                u => u.Id == userId,
                asNoTracking: true,
                u => u.Subscriptie);
            if (utilizator == null)
                return NotFound(new { message = "Utilizator nu gasit" });

            if (!BCrypt.Net.BCrypt.Verify(request.ParolaVeche, utilizator.Parola))
                return BadRequest(new { message = "Parola veche incorecta" });

            utilizator.Parola = BCrypt.Net.BCrypt.HashPassword(request.ParolaNoua);
            await _utilizatorRepository.UpdateAsync(utilizator);

            return Ok(new { message = "Parola schimbata cu succes" });
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

    
}
