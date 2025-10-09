using sprint_final_salud_linux.Data;
using Microsoft.EntityFrameworkCore;
using sprint_final_salud_linux.Services;
using sprint_final_salud_linux.Signal;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MySqlContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36)) // Ajusta a tu versión de MySQL
    )
);

builder.Services.AddSignalR();

builder.Services.AddScoped<CloudinaryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<SignalR>("/turnos");

app.Run();