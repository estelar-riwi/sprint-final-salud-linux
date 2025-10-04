using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace sprint_final_salud_linux.Signal;

public class SignalR : Hub
{
    public async Task SendTurn(int CurrentTurn, int NextTurn)
    {
        await Clients.All.SendAsync("ActualizarTurnos", CurrentTurn, NextTurn);
    }
}