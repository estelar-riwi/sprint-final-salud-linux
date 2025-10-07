using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using sprint_final_salud_linux.Data;
using sprint_final_salud_linux.Models;
using sprint_final_salud_linux.Signal;
namespace sprint_final_salud_linux.Controllers;

public class AdminController : Controller
{
    private readonly MySqlContext _context;
    private readonly IHubContext<SignalR> _hubContext;
    
    //temporal:
    private static int turnoActual = 0;
    
    public AdminController(MySqlContext context, IHubContext<SignalR> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
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
        return Ok();
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

    public IActionResult TurnView()
    {
        var turn = _context.Turns.FirstOrDefault(t => t.Id == 1);

        if (turn == null)
        {
            turn = new Turn { Id = 1, CurrentTurn = 0, NextTurn = 1 };
            _context.Turns.Add(turn);
            _context.SaveChanges();
        }

        return View(turn);
    }

    
    
    [HttpPost, ActionName("TurnView")]
    public async Task<IActionResult> NextTurn()
    {
        // Obtener el único registro de turnos
        var turn = await _context.Turns.FindAsync(1);

        if (turn == null)
        {
            turn = new Turn { Id = 1, CurrentTurn = 0, NextTurn = 1 };
            _context.Turns.Add(turn);
        }

        // Actualizar turno actual y siguiente
        turn.CurrentTurn = (turn.CurrentTurn % 100) + 1;
        turn.NextTurn = (turn.CurrentTurn % 100) + 1;

        await _context.SaveChangesAsync();

        // Notificar a todas las pantallas conectadas
        await _hubContext.Clients.All.SendAsync("ActualizarTurnos", turn.CurrentTurn, turn.NextTurn);

        TempData["message"] = $"Se llamó al turno {turn.CurrentTurn}";
        return RedirectToAction(nameof(Index));
    }
    
    public IActionResult CreateTurnView()
    {
        
        return View();
    }
    

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
            turn = new Turn { Id = 1, CurrentTurn = 0, NextTurn = 1 };
            _context.Turns.Add(turn);
        }
        else
        {
            turn.CurrentTurn = 0;
            turn.NextTurn = 1;
        }

        await _context.SaveChangesAsync();
        
        await _hubContext.Clients.All.SendAsync("ActualizarTurnos", turn.CurrentTurn, turn.NextTurn);

        TempData["message"] = "Los turnos fueron reiniciados correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Infor(int id)
    {
        var userr =  _context.Users.Find(id);
        return View(userr);
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