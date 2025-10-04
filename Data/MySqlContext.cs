using Microsoft.EntityFrameworkCore;
using sprint_final_salud_linux.Models;

namespace sprint_final_salud_linux.Data;

public class MySqlContext : DbContext
{
    public MySqlContext(DbContextOptions<MySqlContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
}