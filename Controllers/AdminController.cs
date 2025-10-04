using Microsoft.AspNetCore.Mvc;
using sprint_final_salud_linux.Data;
using sprint_final_salud_linux.Models;
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
        var users = _context.Users;
        return View(users);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Store([Bind("Name,Identification,Phone")] User user)
    {
        if (ModelState.IsValid)
        {
            var existsIdentification = _context.Users.Any(u => u.Identification == user.Identification);
            if (existsIdentification)
            {
                ModelState.AddModelError("Identification", "Esta identificación ya está registrada.");
                return View("Create", user);
            }
            
            var existisPhone = _context.Users.Any(u => u.Phone == user.Phone);
            if (existisPhone)
            {
                ModelState.AddModelError("phone", "Este telefono ya está registrado.");
                return View("Create", user);           
            }
            
            _context.Add(user);
            _context.SaveChanges();
            TempData["message"] = "Usuario creado";
            return RedirectToAction(nameof(Index));
        }

        return View("Create", user);
    }
    
    public IActionResult Carnet()
    {
        return View();
    }

    public IActionResult Delete(int Id)
    {
        var user = _context.Users.Find(Id);
        if (user == null)
        {
            return NotFound();
        }
        
        _context.Users.Remove(user);
        _context.SaveChanges();
        TempData["message"] = "Cliente eliminado";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int Id)
    {
        var user = _context.Users.Find(Id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    public IActionResult Update(int Id, User updateUser)
    {
        var user = _context.Users.Find(Id);
        if (user == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            user.Name = updateUser.Name;
            user.Identification = updateUser.Identification;
            user.Phone = updateUser.Phone;
            _context.SaveChanges();
            TempData["message"] = "Usuario actualizado";
            return RedirectToAction(nameof(Index));
        }

        return View(updateUser);
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