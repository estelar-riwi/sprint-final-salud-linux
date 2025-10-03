using Microsoft.AspNetCore.Mvc;
using sprint_final_salud_linux.Data;
namespace sprint_final_salud_linux.Controllers;

public class AdminController : Controller
{
    private readonly MySqlContext _context;
    
    public AdminController(MySqlContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Carnet()
    {
        return View();
    }

    public IActionResult Delete()
    {
        return View();
    }

    public IActionResult NextTurn()
    {
        return View();
    }

    public IActionResult ResetTurns()
    {
        return View();
    }

    public IActionResult History()
    {
        return View();
    }

    public IActionResult UserConsultation()
    {
        return View();
    }

    public IActionResult Boxes()
    {
        return View();
    }
}