using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // Agregado para usar las Cookies de Login
using VinotecaApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<VinotecaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VinotecaConnection")));

// Configuramos la seguridad por Cookies para el sistema
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "VinotecaLogin";
        options.LoginPath = "/Account/Login"; // Redirige acá si Leandro intenta entrar sin sesión
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Mantiene la sesión abierta por 7 días
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// EL ORDEN ACÁ ES CLAVE: Primero Authentication, después Authorization
app.UseAuthentication(); 
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();