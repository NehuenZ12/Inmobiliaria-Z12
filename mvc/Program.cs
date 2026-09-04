using mvc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Conexion con PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Add services to the container.
builder.Services.AddControllersWithViews();

// HASHER DE CLAVES (para no guardar texto plano)
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// AUTENTICACIÓN POR COOKIES
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Home/Restringido";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Si no hay usuarios, creamos el primer administrador
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

    if (!db.Usuarios.Any())
    {
        const string emailAdmin = "admin@admin.com";
        const string claveAdmin = "admin123";

        var admin = new Usuario
        {
            Nombre = "Admin",
            Apellido = "Sistema",
            Email = emailAdmin,
            Rol = "Administrador"
        };
        admin.Clave = hasher.HashPassword(admin, claveAdmin);

        db.Usuarios.Add(admin);
        db.SaveChanges();

        // Mostramos las credenciales en consola (solo desarrollo)
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  Primer administrador creado");
            Console.WriteLine($"  Email: {emailAdmin}");
            Console.WriteLine($"  Clave: {claveAdmin}");
            Console.WriteLine("========================================");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Middlewares de autenticación y autorización en el orden correcto
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Propietario}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
