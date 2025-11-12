using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
// --- INICIO DE CAMBIOS ---
using LogisticaBroker.Services; // Para encontrar MailSettings y EmailSender
using Microsoft.AspNetCore.Identity.UI.Services; // La interfaz que Identity a veces usa
// --- FIN DE CAMBIOS ---


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // Lo ponemos en false para facilitar pruebas locales
        options.Password.RequireDigit = false; // Opcional: relajar requisitos de password para desarrollo
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>() // <--- ESTO HABILITA LOS ROLES
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<LogisticaBroker.Services.NotificationService>();


// --- CÓDIGO PARA EL SERVICIO DE CORREO ---
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
// Aquí usamos la interfaz personalizada que creamos en el paso 2
builder.Services.AddTransient<LogisticaBroker.Services.IEmailSender, EmailSender>();
// ------------------------------------------

var app = builder.Build();

// --- INICIO DEL BLOQUE SEEDER ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ejecutamos el método que acabamos de crear
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al ejecutar la siembra de datos (Seeder).");
    }
}
// --- FIN DEL BLOQUE SEEDER ---

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // <--- Es buena práctica tener esto explícitamente
app.UseRouting();

// Ojo: UseAuthentication() debe ir antes de UseAuthorization()
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();