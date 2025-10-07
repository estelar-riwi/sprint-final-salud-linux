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
        var users = _context.Users.Where(u => u.IsActive).ToList();
        ViewBag.Showing = "Activos";
        return View("Index", users);
    }

    // ✅ Mostrar usuarios inactivos
    public IActionResult InactiveList()
    {
        var users = _context.Users.Where(u => !u.IsActive).ToList();
        ViewBag.Showing = "Inactivos";
        return View("Index", users);
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

            user.IsActive = true;
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

    public IActionResult ToggleStatus(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
            return NotFound();

        user.IsActive = !user.IsActive;
        _context.SaveChanges();

        string estado = user.IsActive ? "activado" : "desactivado";
        TempData["message"] = $"Usuario {estado} correctamente";

        return RedirectToAction(user.IsActive ? nameof(Index) : nameof(InactiveList));
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
    public async Task<IActionResult> Update(int Id, User updateUser, string? photoBase64)
    {
        var user = _context.Users.Find(Id);
        if (user == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View("Edit", updateUser);

        // 🔹 Actualizar siempre los datos principales
        user.Name = updateUser.Name;
        user.Identification = updateUser.Identification;
        user.Phone = updateUser.Phone;

        // 🔹 Solo si hay una nueva foto
        if (!string.IsNullOrEmpty(photoBase64))
        {
            try
            {
                var base64Data = photoBase64.Split(',')[1];
                var bytes = Convert.FromBase64String(base64Data);
                user.Picture = await _cloudinary.UploadImageAsync(bytes, $"foto_{DateTime.Now.Ticks}.png");
            }
            catch
            {
                TempData["error"] = "❌ Error al procesar la foto. Intenta nuevamente.";
                return View("Edit", updateUser);
            }
        }

        // 🔹 Guardar cambios siempre
        _context.Update(user);
        await _context.SaveChangesAsync();

        TempData["message"] = "✅ Usuario actualizado correctamente";
        return RedirectToAction(nameof(Index));
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