namespace sprint_final_salud_linux.Models;

public class Turn
{
    public int Id { get; set; }

    public int CurrentTurn { get; set; }

    public int NextTurn { get; set; }
    
    public int TurnRequest { get; set; }
}