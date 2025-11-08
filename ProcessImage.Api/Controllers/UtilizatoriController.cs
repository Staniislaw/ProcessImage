using Data.SDK.Repository;
using ProcessImage.Entities;
using ProcessImage.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using global::Data.SDK.Repository;

namespace ProcessImage.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UtilizatoriController : Controller
    {
        private readonly IRepository<Utilizator> _utilizatorRepository;
        private readonly IRepository<Rol> _rolRepository;
        private readonly IRepository<Subscriptie> _subscriptieRepository;

        public UtilizatoriController(
            IRepository<Utilizator> utilizatorRepository,
            IRepository<Rol> rolRepository,
            IRepository<Subscriptie> subscriptieRepository)
        {
            _utilizatorRepository = utilizatorRepository;
            _rolRepository = rolRepository;
            _subscriptieRepository = subscriptieRepository;
        }

        // GET: Admin/Utilizatori/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var utilizatori = await _utilizatorRepository.GetAllAsync(
                    includes: query => query
                        .Include(u => u.Rol)
                        .Include(u => u.Subscriptie)
                );

                var userListModels = utilizatori.Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    Nume = u.Nume,
                    Email = u.Email,
                    RolNume = u.Rol?.NumeRol ?? "Fara rol",
                    SubscriptieTip = u.Subscriptie?.Tip ?? "Fara subscriptie",
                    SubscriptieId = u.SubscriptieId
                }).ToList();

                return View(userListModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la preluarea utilizatorilor: {ex.Message}");
                return View(new List<UserListViewModel>());
            }
        }

        // GET: Admin/Utilizatori/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var roles = await _rolRepository.GetAllAsync();
                var subscriptions = await _subscriptieRepository.GetAllAsync();

                ViewBag.Roles = roles;
                ViewBag.Subscriptions = subscriptions;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la încarcarea datelor create: {ex.Message}");
                ModelState.AddModelError("", "Eroare la încarcarea datelor");
                return View();
            }
        }

        // POST: Admin/Utilizatori/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _rolRepository.GetAllAsync();
                var subscriptions = await _subscriptieRepository.GetAllAsync();
                ViewBag.Roles = roles;
                ViewBag.Subscriptions = subscriptions;
                return View(model);
            }

            try
            {
                var existingUser = await _utilizatorRepository.GetAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email-ul este deja folosit");
                    var roles = await _rolRepository.GetAllAsync();
                    var subscriptions = await _subscriptieRepository.GetAllAsync();
                    ViewBag.Roles = roles;
                    ViewBag.Subscriptions = subscriptions;
                    return View(model);
                }

                var rol = await _rolRepository.GetAsync(r => r.Id == model.RolId);
                var subscriptie = await _subscriptieRepository.GetAsync(s => s.Id == model.SubscriptieId);

                if (rol == null || subscriptie == null)
                {
                    ModelState.AddModelError("", "Rol sau subscriptie invalid");
                    var rolesData = await _rolRepository.GetAllAsync();
                    var subscriptionsData = await _subscriptieRepository.GetAllAsync();
                    ViewBag.Roles = rolesData;
                    ViewBag.Subscriptions = subscriptionsData;
                    return View(model);
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Parola);
                var noiUtilizator = new Utilizator
                {
                    Nume = model.Nume,
                    Email = model.Email,
                    Parola = hashedPassword,
                    RolId = model.RolId,
                    SubscriptieId = model.SubscriptieId,
                };
                await _utilizatorRepository.AddAsync(noiUtilizator);
                await _utilizatorRepository.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Utilizatorul {model.Nume} a fost creat cu succes!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Eroare la crearea utilizatorului: {ex.Message}");
                var roles = await _rolRepository.GetAllAsync();
                var subscriptions = await _subscriptieRepository.GetAllAsync();
                ViewBag.Roles = roles;
                ViewBag.Subscriptions = subscriptions;
                return View(model);
            }
        }

        // GET: Admin/Utilizatori/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(long id)
        {
            try
            {
                var utilizator = await _utilizatorRepository.GetAsync(
                    u => u.Id == id,
                    includes: query => query
                        .Include(u => u.Rol)
                        .Include(u => u.Subscriptie)
                );

                if (utilizator == null)
                {
                    return NotFound();
                }

                var roles = await _rolRepository.GetAllAsync();
                var subscriptions = await _subscriptieRepository.GetAllAsync();

                ViewBag.Roles = roles;
                ViewBag.Subscriptions = subscriptions;

                return View(utilizator);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la editarea utilizatorului: {ex.Message}");
                return NotFound();
            }
        }

        // GET: Admin/Utilizatori/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var utilizator = await _utilizatorRepository.GetAsync(
                    u => u.Id == id,
                    includes: query => query
                        .Include(u => u.Rol)
                        .Include(u => u.Subscriptie)
                );

                if (utilizator == null)
                {
                    return NotFound();
                }

                return View(utilizator);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la stergerea utilizatorului: {ex.Message}");
                return NotFound();
            }
        }

        // POST: Admin/Utilizatori/Delete/5
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var utilizator = await _utilizatorRepository.GetAsync(u => u.Id == id);
                if (utilizator == null)
                {
                    return NotFound();
                }

                await _utilizatorRepository.DeleteAsync(utilizator);
                await _utilizatorRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Utilizatorul {utilizator.Nume} a fost sters cu succes!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la stergerea utilizatorului: {ex.Message}");
                TempData["ErrorMessage"] = "Eroare la stergerea utilizatorului";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("EditUser/{id}")]
        public async Task<IActionResult> EditUser(long id)
        {
            try
            {
                var utilizator = await _utilizatorRepository.GetAsync(
                    u => u.Id == id,
                    includes: query => query
                        .Include(u => u.Rol)
                        .Include(u => u.Subscriptie)
                );
                if (utilizator == null)
                {
                    return NotFound();
                }
                var model = new EditUserViewModel
                {
                    Id = utilizator.Id,
                    Nume = utilizator.Nume,
                    Email = utilizator.Email,
                    RolId = utilizator.RolId ?? 0
                };
                var roles = await _rolRepository.GetAllAsync();
                ViewBag.Roles = roles;
                return View("Edit", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la editarea utilizatorului: {ex.Message}");
                return NotFound();
            }
        }

        [HttpPost("UpdateUser/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(long id, EditUserViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                var roles = await _rolRepository.GetAllAsync();
                ViewBag.Roles = roles;
                return View("Edit", model);
            }

            try
            {
                var utilizator = await _utilizatorRepository.GetAsync(u => u.Id == id);
                if (utilizator == null)
                    return NotFound();

                var existingEmail = await _utilizatorRepository.GetAsync(
                    u => u.Email == model.Email && u.Id != id
                );

                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Acest email este deja folosit");
                    var roles = await _rolRepository.GetAllAsync();
                    ViewBag.Roles = roles;
                    return View("Edit", model);
                }

                utilizator.Nume = model.Nume;
                utilizator.Email = model.Email;
                utilizator.RolId = model.RolId;

                if (!string.IsNullOrWhiteSpace(model.NovaParola))
                {
                    utilizator.Parola = BCrypt.Net.BCrypt.HashPassword(model.NovaParola);
                }

                await _utilizatorRepository.UpdateAsync(utilizator);
                await _utilizatorRepository.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Utilizatorul {model.Nume} a fost actualizat cu succes!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la editarea utilizatorului: {ex.Message}");
                ModelState.AddModelError("", $"Eroare la actualizarea utilizatorului: {ex.Message}");
                var roles = await _rolRepository.GetAllAsync();
                ViewBag.Roles = roles;
                return View("Edit", model);
            }
        }
    }
}
