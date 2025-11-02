using Data.SDK.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ProcessImage.Entities;
using ProcessImage.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
[Area("Admin")]
[Route("Admin/[controller]")]
public class AuthController : Controller
{
    private readonly IRepository<Utilizator> _utilizatorRepository;

    public AuthController(IRepository<Utilizator> utilizatorRepository)
    {
        _utilizatorRepository = utilizatorRepository;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Verifică dacă utilizatorul este deja autentificat ca Admin
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "DashboardAdmin", new { area = "Admin" });
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        // Debug: verifică ce date vin
        Console.WriteLine($"Email: {model?.Email}, Parola: {model?.Parola}");

        if (!ModelState.IsValid)
        {
            // Afișează erorile de model
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Model error: {error.ErrorMessage}");
            }
            return View(model);
        }

        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Parola))
        {
            ModelState.AddModelError("", "Email și parola sunt obligatorii");
            return View(model);
        }

        try
        {
            var utilizator = await _utilizatorRepository.GetAsync(
                u => u.Email == model.Email,
                includes: query => query.Include(u => u.Rol)
            );

            if (utilizator == null)
            {
                ModelState.AddModelError("", "Email sau parolă incorectă");
                return View(model);
            }

            // Verifică parola
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Parola, utilizator.Parola);
            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Email sau parolă incorectă");
                return View(model);
            }

            if (utilizator.Rol?.NumeRol != "Admin")
            {
                ModelState.AddModelError("", "Nu aveți permisiuni de administrator");
                return View(model);
            }

            // Creare claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilizator.Id.ToString()),
                new Claim(ClaimTypes.Name, utilizator.Nume),
                new Claim(ClaimTypes.Email, utilizator.Email),
                new Claim(ClaimTypes.Role, utilizator.Rol.NumeRol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "DashboardAdmin", new { area = "Admin" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare la login: {ex.Message}");
            ModelState.AddModelError("", "A apărut o eroare la autentificare");
            return View(model);
        }
    }

    [HttpGet("Logout")]
    public async Task<IActionResult> LogoutGet()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");

    }
    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");

    }
}