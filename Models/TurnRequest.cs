namespace sprint_final_salud_linux.Models;

public class TurnRequest
{
    public int Id { get; set; }

    public int Number { get; set; }
    public string? Window { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsServed { get; set; } = false;
}