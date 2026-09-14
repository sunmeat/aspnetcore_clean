using Soccer.Application.DependencyInjection;
using Soccer.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// отримуємо рядок підключення з конфігурації (там треба налаштувати правильне джерело в appsettings.json)
string? connection = builder.Configuration.GetConnectionString("DefaultConnection");

// Program.cs - єдине місце (композиційний корінь), де Presentation знає і про
// Application, і про Infrastructure одночасно. Кожен шар реєструє сам себе
// через власний Add-метод, а тут вони просто збираються докупи.
builder.Services.AddInfrastructure(connection); // реєструємо DbContext та Unit of Work (Infrastructure)
builder.Services.AddApplication();              // реєструємо AutoMapper та сервіси (Application)

// додаємо сервіси MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles(); // запити до статичних файлів у wwwroot

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Teams}/{action=Index}/{id?}"); // налаштовуємо стандартний маршрут

app.Run();