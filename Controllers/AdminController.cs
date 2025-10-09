using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using sprint_final_salud_linux.Data;
using sprint_final_salud_linux.Models;
using sprint_final_salud_linux.Signal;
using sprint_final_salud_linux.Services;

namespace sprint_final_salud_linux.Controllers
{
    public class AdminController : Controller
    {
        private readonly MySqlContext _context;
        private readonly IHubContext<SignalR> _hubContext;
        private readonly CloudinaryService _cloudinary;

        private static int turnoActual = 0;

        public AdminController(MySqlContext context, CloudinaryService cloudinary, IHubContext<SignalR> hubContext)
        {
            _context = context;
            _cloudinary = cloudinary;
            _hubContext = hubContext;
        }

        // ✅ Mostrar usuarios activos
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

        // ✅ Vista crear afiliado
        public IActionResult Create()
        {
            return View();
        }

        // ✅ Guardar nuevo afiliado
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

                // 📷 Subir foto si existe
                if (!string.IsNullOrEmpty(photoBase64))
                {
                    var base64Data = photoBase64.Split(',')[1];
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

        // ✅ Ver carnet
        public IActionResult Carnet()
        {
            return View();
        }

        // ✅ Cambiar estado activo/inactivo
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

        // ✅ Vista editar afiliado
        public IActionResult Edit(int Id)
        {
            var user = _context.Users.Find(Id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // ✅ Actualizar afiliado
        [HttpPost]
        public async Task<IActionResult> Update(int Id, User updateUser, string? photoBase64)
        {
            var user = _context.Users.Find(Id);
            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View("Edit", updateUser);

            // 🔹 Actualizar datos principales
            user.Name = updateUser.Name;
            user.Identification = updateUser.Identification;
            user.Phone = updateUser.Phone;

            // 🔹 Solo si hay nueva foto
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

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["message"] = "✅ Usuario actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Vista del turno actual
        public IActionResult TurnView()
        {
            var turn = _context.Turns.FirstOrDefault(t => t.Id == 1);

            if (turn == null)
            {
                turn = new Turn { Id = 1, CurrentTurn = 0, NextTurn = 1, TurnRequest = 1 };
                _context.Turns.Add(turn);
                _context.SaveChanges();
            }

            return View(turn);
        }

        // ✅ Llamar siguiente turno
        [HttpPost, ActionName("TurnView")]
        public async Task<IActionResult> NextTurn(string windowName)
        {
            // Si no se envía ventanilla, asignar por defecto
            windowName ??= "Caja 1";

            var turn = await _context.Turns.FindAsync(1);

            if (turn == null)
            {
                turn = new Turn { Id = 1, CurrentTurn = 0, NextTurn = 1, TurnRequest = 1 };
                _context.Turns.Add(turn);
                await _context.SaveChangesAsync();
            }

            // Actualizamos el turno actual
            turn.CurrentTurn = turn.NextTurn;
            turn.NextTurn = (turn.NextTurn % 100) + 1;

            // ✅ Buscar si ese turno ya existe en TurnRequests
            var existingRequest = _context.TurnRequests.FirstOrDefault(t => t.Number == turn.CurrentTurn);
            if (existingRequest != null)
            {
                existingRequest.IsServed = true;
                existingRequest.Window = windowName;
            }
            else
            {
                // Si no existe, crearlo por seguridad
                var newRequest = new TurnRequest
                {
                    Number = turn.CurrentTurn,
                    IsServed = true,
                    Window = windowName,
                    CreatedAt = DateTime.Now
                };
                _context.TurnRequests.Add(newRequest);
            }

            await _context.SaveChangesAsync();

            // Notificar al panel de turnos
            await _hubContext.Clients.All.SendAsync("ActualizarTurnos",
                turn.CurrentTurn,
                turn.NextTurn,
                windowName
            );

            TempData["message"] = $"Se llamó al turno {turn.CurrentTurn} en {windowName}";
            return RedirectToAction(nameof(Index));
        }


        // ✅ Crear turno manualmente
        public IActionResult CreateTurnView()
        {
            return View();
        }

        // ✅ Reiniciar turnos
        public IActionResult ResetTurns()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetTurnsConfirmed()
        {
            var turn = await _context.Turns.FindAsync(1);

            if (turn == null)
            {
                turn = new Turn { Id = 1, CurrentTurn = 0, NextTurn = 1, TurnRequest = 1};
                _context.Turns.Add(turn);
            }
            else
            {
                turn.CurrentTurn = 0;
                turn.NextTurn = 1;
                turn.TurnRequest = 1;
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ActualizarTurnos", turn.CurrentTurn, turn.NextTurn);

            TempData["message"] = "Los turnos fueron reiniciados correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Otras vistas
        public IActionResult Historial()
        {
            var historial = _context.TurnRequests
                .OrderByDescending(h => h.CreatedAt)
                .ToList();

            return View(historial);
        }

        public IActionResult UserConsultation() => View();

        public IActionResult Boxes() => View();
    }
}
