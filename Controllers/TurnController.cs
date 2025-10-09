using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using sprint_final_salud_linux.Data;
using sprint_final_salud_linux.Models;
using sprint_final_salud_linux.Signal;

namespace sprint_final_salud_linux.Controllers;
public class TurnController : Controller
{
    private readonly MySqlContext _context;
    private readonly IHubContext<SignalR> _hubContext;

    public TurnController(MySqlContext context,IHubContext<SignalR> hubContext){
        _context=context;
        _hubContext=hubContext;
    }

    public IActionResult RequestTurn(){ return View(); }

    [HttpPost]
    public async Task<IActionResult> RequestTurnPost(){
        
        PrinterController printerController = new PrinterController(_context);
        printerController.PrintTurn();
        
        var turn = _context.Turns.FirstOrDefault(t=>t.Id==1);
        if(turn==null){
            turn = new Turn{Id=1,CurrentTurn=0,NextTurn=1,TurnRequest=1};
            _context.Turns.Add(turn);
            await _context.SaveChangesAsync();
        }

        int assigned = turn.TurnRequest;

        var request = new TurnRequest{Number=assigned};
        _context.TurnRequests.Add(request);

        turn.TurnRequest = (turn.TurnRequest % 100) +1;
        await _context.SaveChangesAsync();
        
        await _hubContext.Clients.All.SendAsync("NuevoTurnoSolicitado", assigned);
        
        return Json(new { turno = assigned });
    }
    public IActionResult TurnConfirmation(){ return View(); }

    public IActionResult TurnView(){
        var turn = _context.Turns.FirstOrDefault(t=>t.Id==1);
        if(turn==null){ turn = new Turn{Id=1,CurrentTurn=0,NextTurn=1,TurnRequest=1}; }
        return View(turn);
    }


    [HttpPost]
    public async Task<IActionResult> ResetTurnsConfirmed()
    {
        var turn = _context.Turns.FirstOrDefault(t => t.Id == 1);
        if (turn != null)
        {
            turn.CurrentTurn = 0;
            turn.NextTurn = 1;
            turn.TurnRequest = 1;
            await _context.SaveChangesAsync();
        }

        await _hubContext.Clients.All.SendAsync("ActualizarTurnos", 0, 1, "-");
        TempData["message"] = "Turnos reiniciados correctamente.";
        return RedirectToAction("ResetTurns");
    }

    public IActionResult ResetTurns(){ return View(); }
}