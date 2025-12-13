using Data.SDK.Repository;
using ProcessImage.Entities;
using ProcessImage.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using global::Data.SDK.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using ProcessImage.Services.Interface;

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
        private readonly ICacheService _cacheService;

        private const string CACHE_KEY_ALL_USERS = "admin_all_users";
        private const string CACHE_KEY_USER_ID = "admin_user_{0}";
        private const string CACHE_KEY_ALL_ROLES = "admin_all_roles";
        private const string CACHE_KEY_ALL_SUBSCRIPTIONS = "admin_all_subscriptions";

        private const string CACHE_PATTERN_USERS = "admin_user_";
        private const string CACHE_PATTERN_ADMIN = "admin_";

        public UtilizatoriController(
            IRepository<Utilizator> utilizatorRepository,
            IRepository<Rol> rolRepository,
            IRepository<Subscriptie> subscriptieRepository,
            ICacheService cacheService)
        {
            _utilizatorRepository = utilizatorRepository;
            _rolRepository = rolRepository;
            _subscriptieRepository = subscriptieRepository;
            _cacheService = cacheService;

        }

        // GET: Admin/Utilizatori/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (_cacheService.Exists(CACHE_KEY_ALL_USERS))
                {
                    var cachedUsers = _cacheService.Get<List<UserListViewModel>>(CACHE_KEY_ALL_USERS);
                    return View(cachedUsers);
                }

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

                _cacheService.Set(CACHE_KEY_ALL_USERS, userListModels, TimeSpan.FromMinutes(20));

                return View(userListModels);
            }
            catch (Exception ex)
            {
                return View(new List<UserListViewModel>());
            }
        }

        // GET: Admin/Utilizatori/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new CreateUserViewModel
                {
                    Roluri = (await _rolRepository.GetAllAsync())
                                .Select(r => new SelectListItem
                                {
                                    Text = r.NumeRol,
                                    Value = r.Id.ToString()
                                }).ToList(),

                    Subscriptii = (await _subscriptieRepository.GetAllAsync())
                                .Select(s => new SelectListItem
                                {
                                    Text = s.Tip,
                                    Value = s.Id.ToString()
                                }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Eroare la incarcarea datelor");
                return View(new CreateUserViewModel());
            }
        }

        // POST: Admin/Utilizatori/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roluri = (await _rolRepository.GetAllAsync())
                                    .Select(r => new SelectListItem
                                    {
                                        Text = r.NumeRol,
                                        Value = r.Id.ToString()
                                    }).ToList();

                model.Subscriptii = (await _subscriptieRepository.GetAllAsync())
                                    .Select(s => new SelectListItem
                                    {
                                        Text = s.Tip,
                                        Value = s.Id.ToString()
                                    }).ToList();

                return View(model);
            }

            try
            {
                var existingUser = await _utilizatorRepository.GetAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email-ul este deja folosit");
                    return View(model);
                }

                var rol = await _rolRepository.GetAsync(r => r.Id == model.RolId);
                var subscriptie = await _subscriptieRepository.GetAsync(s => s.Id == model.SubscriptieId);

                if (rol == null || subscriptie == null)
                {
                    ModelState.AddModelError("", "Rol sau subscriptie invalid");
                    return View(model);
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Parola ?? "");
                var nouUtilizator = new Utilizator
                {
                    Nume = model.Nume,
                    Email = model.Email,
                    Parola = hashedPassword,
                    RolId = model.RolId,
                    SubscriptieId = model.SubscriptieId
                };

                await _utilizatorRepository.AddAsync(nouUtilizator);
                await _utilizatorRepository.SaveChangesAsync();

                _cacheService.Remove(CACHE_KEY_ALL_USERS);

                TempData["SuccessMessage"] = $"Utilizatorul {model.Nume} a fost creat cu succes!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Eroare la crearea utilizatorului: {ex.Message}");
                return View(model);
            }
        }

        // GET: Admin/Utilizatori/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(long id)
        {
            try
            {
                string cacheKey = string.Format(CACHE_KEY_USER_ID, id);

                // 1. VERIFICA cache pentru user-ul specific
                Utilizator utilizator = null;
                if (_cacheService.Exists(cacheKey))
                {
                    utilizator = _cacheService.Get<Utilizator>(cacheKey);
                }
                else
                {
                    // 2. PREIA din baza de date
                    utilizator = await _utilizatorRepository.GetAsync(
                        u => u.Id == id,
                        includes: query => query
                            .Include(u => u.Rol)
                            .Include(u => u.Subscriptie)
                    );

                    if (utilizator != null)
                    {
                        // 3. ADAUGĂ în cache (15 minute)
                        _cacheService.Set(cacheKey, utilizator, TimeSpan.FromMinutes(15));
                    }
                }

                if (utilizator == null)
                {
                    return NotFound();
                }

                var roles = await GetRolesWithCacheAsync();
                var subscriptions = await GetSubscriptionsWithCacheAsync();

                ViewBag.Roles = roles;
                ViewBag.Subscriptions = subscriptions;

                var model = new EditUserViewModel
                {
                    Id = utilizator.Id,
                    Nume = utilizator.Nume,
                    Email = utilizator.Email,
                    RolId = utilizator.RolId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var utilizator = await _utilizatorRepository.GetAsync(u => u.Id == id);

            if (utilizator == null)
                return NotFound();

            utilizator.isActive = false;

            await _utilizatorRepository.UpdateAsync(utilizator);

            _cacheService.Remove(CACHE_KEY_ALL_USERS);
            _cacheService.Remove(string.Format(CACHE_KEY_USER_ID, id));

            TempData["SuccessMessage"] = $"Utilizatorul {utilizator.Nume} a fost dezactivat cu succes!";
            return RedirectToAction("Index");
        }


        [HttpGet("EditUser/{id}")]
        public async Task<IActionResult> EditUser(long id)
        {
            try
            {
                string cacheKey = string.Format(CACHE_KEY_USER_ID, id);

                // 1. VERIFICA cache
                Utilizator utilizator = null;
                if (_cacheService.Exists(cacheKey))
                {
                    utilizator = _cacheService.Get<Utilizator>(cacheKey);
                }
                else
                {
                    utilizator = await _utilizatorRepository.GetAsync(
                        u => u.Id == id,
                        includes: query => query
                            .Include(u => u.Rol)
                            .Include(u => u.Subscriptie)
                    );

                    if (utilizator != null)
                    {
                        _cacheService.Set(cacheKey, utilizator, TimeSpan.FromMinutes(15));
                    }
                }

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

                var roles = await GetRolesWithCacheAsync();
                ViewBag.Roles = roles;

                return View("Edit", model);
            }
            catch (Exception ex)
            {
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
                var roles = await GetRolesWithCacheAsync();
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
                    var roles = await GetRolesWithCacheAsync();
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

                _cacheService.Remove(CACHE_KEY_ALL_USERS);
                _cacheService.Remove(string.Format(CACHE_KEY_USER_ID, id));

                TempData["SuccessMessage"] = $"Utilizatorul {model.Nume} a fost actualizat cu succes!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Eroare la actualizarea utilizatorului: {ex.Message}");
                var roles = await GetRolesWithCacheAsync();
                ViewBag.Roles = roles;
                return View("Edit", model);
            }
        }
        private async Task<List<Rol>> GetRolesWithCacheAsync()
        {
            if (_cacheService.Exists(CACHE_KEY_ALL_ROLES))
            {
                return _cacheService.Get<List<Rol>>(CACHE_KEY_ALL_ROLES);
            }

            var roles = (await _rolRepository.GetAllAsync()).ToList();
            _cacheService.Set(CACHE_KEY_ALL_ROLES, roles, TimeSpan.FromHours(1));

            return roles;
        }
        private async Task<List<Subscriptie>> GetSubscriptionsWithCacheAsync()
        {
            if (_cacheService.Exists(CACHE_KEY_ALL_SUBSCRIPTIONS))
            {
                return _cacheService.Get<List<Subscriptie>>(CACHE_KEY_ALL_SUBSCRIPTIONS);
            }

            var subscriptions = (await _subscriptieRepository.GetAllAsync()).ToList();
            _cacheService.Set(CACHE_KEY_ALL_SUBSCRIPTIONS, subscriptions, TimeSpan.FromHours(1));

            return subscriptions;
        }

    }
}
