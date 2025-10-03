using Microsoft.EntityFrameworkCore;
namespace sprint_final_salud_linux.Data;

public class MySqlContext : DbContext
{
    public MySqlContext(DbContextOptions<MySqlContext> options)
        : base(options)
    {
    }
}