using eduMentor.Data;
using eduMentor.Models;
using eduMentor.Filters;
using eduMentor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
// ===========================================
// 🔹 1. Configuración de base de datos
// ===========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ===========================================
// 🔹 2. Configuración de Identity
// ===========================================
builder.Services.AddIdentity<Usuario, Role>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// ===========================================
// 🔹 3. Servicios auxiliares y sesión
// ===========================================
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// ===========================================
// 🔹 4. MVC y Razor Pages
// ===========================================
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<eduMentor.Filters.SidebarStateFilter>();
});

builder.Services.AddRazorPages();

var app = builder.Build();

// ===========================================
// 🔹 5. Middleware base
// ===========================================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value?.ToLower();

        if (string.IsNullOrEmpty(path) || path == "/" || path.StartsWith("/home"))
        {
            bool tieneSesion = context.User.Identity.IsAuthenticated ;

            if (tieneSesion)
                context.Response.Redirect("/Pwa/Index");
            else
                context.Response.Redirect("/Landing/Index");

            return;
        }

        await next();
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ===========================================
// 🔹 6. Pipeline estándar
// ===========================================
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ===========================================
// 🔹 7. Rutas personalizadas
// ===========================================
app.MapControllerRoute(
    name: "landing",
    pattern: "Landing/{action=Index}/{id?}",
    defaults: new { controller = "Landing" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "pwa",
    pattern: "Pwa/{controller=Pwa}/{action=Index}/{id?}",
    defaults: new { controller = "Pwa" }
);

app.MapRazorPages();

// ===========================================
// 🔹 8. Semilla de roles y usuario admin
// ===========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        app.Logger.LogInformation("📦 Aplicando migraciones pendientes...");
        db.Database.Migrate(); 
        app.Logger.LogInformation("✅ Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Error aplicando migraciones: {Message}", ex.Message);
    }

    try
    {
        await SeedRolesAndAdmin(services);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "⚠️ Error al crear roles o admin: {Message}", ex.Message);
    }
}


app.Run();

// ===========================================
// 🔹 Método auxiliar de semilla
// ===========================================


async Task SeedRolesAndAdmin(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<Role>>();
    var userManager = services.GetRequiredService<UserManager<Usuario>>();

    string[] roles = { "Administrador", "Instructor", "Alumno" };

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new Role { Name = roleName, Descripcion = $"Rol {roleName}" });
    }

    app.Logger.LogInformation("⏳ Iniciando semilla de roles y usuario admin...");

    var adminEmail = "admin@edumentor.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        app.Logger.LogInformation("👤 Usuario admin no existe, se creará...");

        admin = new Usuario
        {
            UserName = adminEmail,
            Email = adminEmail,
            Nombre = "Administrador del sistema",
            Activo = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123");

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                app.Logger.LogWarning($"❌ Error creando usuario admin: {error.Code} - {error.Description}");
        }
        else
        {
            await userManager.AddToRoleAsync(admin, "Administrador");
            app.Logger.LogInformation("✅ Usuario admin creado y asignado correctamente.");
        }
    }
    else
    {
        app.Logger.LogInformation("ℹ️ Usuario admin ya existe, no se recreará.");
    }

}
