using Microsoft.AspNetCore.Mvc;
using sprint_final_salud_linux.Data;
using sprint_final_salud_linux.Models;
using sprint_final_salud_linux.Services;

namespace sprint_final_salud_linux.Controllers;

public class AdminController : Controller
{
     private readonly MySqlContext _context;
    private readonly CloudinaryService _cloudinary;

    public AdminController(MySqlContext context, CloudinaryService cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
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
    public async Task<IActionResult> Store([Bind("Name,Identification,Phone")] User user, string photoBase64)
    {
        if (ModelState.IsValid)
        {
            // Validaciones
            if (_context.Users.Any(u => u.Identification == user.Identification))
            {
                ModelState.AddModelError("Identification", "Esta identificación ya está registrada.");
                return View("Create", user);
            }

            if (_context.Users.Any(u => u.Phone == user.Phone))
            {
                ModelState.AddModelError("Phone", "Este teléfono ya está registrado.");
                return View("Create", user);
            }

            // 📷 Si llegó la foto en base64, subir a Cloudinary
            if (!string.IsNullOrEmpty(photoBase64))
            {
                var base64Data = photoBase64.Split(',')[1]; // quitar "data:image/png;base64,"
                var bytes = Convert.FromBase64String(base64Data);

                user.Picture = await _cloudinary.UploadImageAsync(bytes, $"foto_{DateTime.Now.Ticks}.png");
            }

            _context.Add(user);
            await _context.SaveChangesAsync();

            TempData["message"] = "Usuario registrado correctamente";
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

    [HttpPost]
    public async Task<IActionResult> Update(int Id, User updateUser, string photoBase64)
    {
        var user = _context.Users.Find(Id);
        if (user == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            user.Name = updateUser.Name;
            user.Identification = updateUser.Identification;
            user.Phone = updateUser.Phone;
            
            if (!string.IsNullOrEmpty(photoBase64))
            {
                var base64Data = photoBase64.Split(',')[1];
                var bytes = Convert.FromBase64String(base64Data);
                user.Picture = await _cloudinary.UploadImageAsync(bytes, $"foto_{DateTime.Now.Ticks}.png");
            }

            await _context.SaveChangesAsync();
            TempData["message"] = "Usuario actualizado correctamente";
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