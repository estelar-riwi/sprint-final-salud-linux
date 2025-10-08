using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using sprint_final_salud_linux.Data;
using sprint_final_salud_linux.Models;
using sprint_final_salud_linux.Signal;
namespace sprint_final_salud_linux.Controllers;

public class TurnController :Controller
{
    private readonly MySqlContext _context;
    private readonly IHubContext<SignalR> _hubContext;

    public TurnController(MySqlContext context, IHubContext<SignalR> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public IActionResult RequestTurn()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RequestTurnPost()
    {
        var turn = _context.Turns.FirstOrDefault(t => t.Id == 1);
        if (turn == null)
        {
            turn = new Turn { Id = 1, CurrentTurn = 0, NextTurn = 1 };
            _context.Turns.Add(turn);
            await _context.SaveChangesAsync();
        }
        int assigned = turn.NextTurn;
        
        var request = new TurnRequest
        {
            Number = assigned,
        };
        _context.TurnRequests.Add(request);
        
        turn.NextTurn = (turn.NextTurn % 100) + 1;

        await _context.SaveChangesAsync();

        TempData["assignedTurn"] = assigned;

        // Avisar a todos que hay un nuevo turno pedido
        /*await _hubContext.Clients.All.SendAsync("NuevoTurnoSolicitado", assigned);*/
        /*PrinterController printnn = new PrinterController(_context);
        printnn.PrintTurn(assigned);*/
        RedirectToAction("PrintTurn", "Printer", new { prnTurn = assigned });
        return RedirectToAction("TurnConfirmation");
    }

    public IActionResult TurnConfirmation()
    {
        ViewBag.Turno = TempData["assignedTurn"];
        return View();
    }
}
