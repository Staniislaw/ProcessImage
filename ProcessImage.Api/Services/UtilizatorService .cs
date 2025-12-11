using Data.SDK.Repository;
using Microsoft.IdentityModel.Tokens;
using ProcessImage.Entities;
using ProcessImage.Models.DTO;
using ProcessImage.Models;
using ProcessImage.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProcessImage.Services
{
    public class UtilizatorService : IUtilizatorService
    {
        private readonly IRepository<Utilizator> _utilizatorRepository;
        private readonly IRepository<Subscriptie> _subscriptieRepository;
        private readonly IConfiguration _configuration;

        public UtilizatorService(
            IRepository<Utilizator> utilizatorRepository,
            IRepository<Subscriptie> subscriptieRepository,
            IConfiguration configuration)
        {
            _utilizatorRepository = utilizatorRepository;
            _subscriptieRepository = subscriptieRepository;
            _configuration = configuration;
        }

        public async Task<object> RegisterAsync(RegisterRequest request)
        {
            var utilizatorExistent = await _utilizatorRepository.GetAsync(u => u.Email == request.Email);
            if (utilizatorExistent != null)
                throw new InvalidOperationException("Utilizatorul cu acest email exista deja");

            var subscriptieFree = await _subscriptieRepository.GetAsync(s => s.Tip == "Free");
            if (subscriptieFree == null)
                throw new InvalidOperationException("Subscriptia Free nu este configurata în sistem");

            var parolaHashata = BCrypt.Net.BCrypt.HashPassword(request.Parola);

            var utilizator = new Utilizator
            {
                Nume = request.Nume,
                Email = request.Email,
                Parola = parolaHashata,
                SubscriptieId = subscriptieFree.Id
            };

            var result = await _utilizatorRepository.AddAsync(utilizator);
            return new
            {
                message = "Utilizator inregistrat cu succes",
                utilizatorId = result.Id,
                subscriptie = subscriptieFree.Tip
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var utilizator = await _utilizatorRepository.GetAsync(u => u.Email == request.Email);
            if (utilizator == null)
                throw new UnauthorizedAccessException("Email sau parola incorecta");

            var isValid = BCrypt.Net.BCrypt.Verify(request.Parola, utilizator.Parola);
            if (!isValid)
                throw new UnauthorizedAccessException("Email sau parola incorecta");

            var token = GenerateJwtToken(utilizator);
            return new LoginResponse
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
        }

        public async Task<ProfilResponse> GetProfilAsync(int userId)
        {
            var utilizator = await _utilizatorRepository.GetAsync(u => u.Id == userId);
            if (utilizator == null)
                throw new KeyNotFoundException("Utilizator nu gasit");

            return new ProfilResponse
            {
                Id = utilizator.Id,
                Nume = utilizator.Nume,
                Email = utilizator.Email,
                SubscriptieId = utilizator.SubscriptieId
            };
        }

        public async Task UpdateProfilAsync(int userId, UpdateProfilRequest request)
        {
            var utilizator = await _utilizatorRepository.GetSingleWhereIncludeAsync(
                u => u.Id == userId,
                asNoTracking: true,
                u => u.Subscriptie);

            if (utilizator == null)
                throw new KeyNotFoundException("Utilizator nu gasit");

            utilizator.Nume = request.Nume ?? utilizator.Nume;
            await _utilizatorRepository.UpdateAsync(utilizator);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var utilizator = await _utilizatorRepository.GetSingleWhereIncludeAsync(
                u => u.Id == userId,
                asNoTracking: true,
                u => u.Subscriptie);

            if (utilizator == null)
                throw new KeyNotFoundException("Utilizator nu gasit");

            if (!BCrypt.Net.BCrypt.Verify(request.ParolaVeche, utilizator.Parola))
                throw new InvalidOperationException("Parola veche incorecta");

            utilizator.Parola = BCrypt.Net.BCrypt.HashPassword(request.ParolaNoua);
            await _utilizatorRepository.UpdateAsync(utilizator);
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

