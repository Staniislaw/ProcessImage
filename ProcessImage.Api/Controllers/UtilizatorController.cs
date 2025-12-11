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
        private readonly IUtilizatorService _utilizatorService;
        private readonly IBaseService _baseService;

        public UtilizatorController(IUtilizatorService utilizatorService, IBaseService baseService)
        {
            _utilizatorService = utilizatorService;
            _baseService = baseService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _utilizatorService.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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

            try
            {
                var response = await _utilizatorService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Eroare la logare", details = ex.Message });
            }
        }

        [HttpGet("profil")]
        public async Task<IActionResult> GetProfil()
        {
            try
            {
                var userId = _baseService.GetUserId();
                var profil = await _utilizatorService.GetProfilAsync(userId);
                return Ok(profil);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Eroare la preluarea profilului", details = ex.Message });
            }
        }

        [HttpPut("update-profil")]
        public async Task<IActionResult> UpdateProfil([FromBody] UpdateProfilRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _baseService.GetUserId();
                await _utilizatorService.UpdateProfilAsync(userId, request);
                return Ok(new { message = "Profil actualizat cu succes" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Eroare la actualizare", details = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _baseService.GetUserId();
                await _utilizatorService.ChangePasswordAsync(userId, request);
                return Ok(new { message = "Parola schimbata cu succes" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Eroare la schimbarea parolei", details = ex.Message });
            }
        }
    }
}
