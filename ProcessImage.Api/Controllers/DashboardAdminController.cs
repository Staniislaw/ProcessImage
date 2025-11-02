using Data.SDK.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ProcessImage.Entities;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardAdminController : Controller
{
    private readonly IRepository<Utilizator> _utilizatorRepository;

    public DashboardAdminController(IRepository<Utilizator> utilizatorRepository)
    {
        _utilizatorRepository = utilizatorRepository;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Panou de Administrare";
        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await _utilizatorRepository.GetAllAsync(
            includes: query => query.Include(u => u.Subscriptie)
        );
        return View(users);
    }

    [HttpGet]
    public IActionResult Setari()
    {
        return View();
    }
}