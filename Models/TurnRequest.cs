namespace sprint_final_salud_linux.Models;

public class TurnRequest
{
    public int Id { get; set; }

    public int Number { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
}