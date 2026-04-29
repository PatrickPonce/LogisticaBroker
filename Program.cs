using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using LogisticaBroker.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

// Carga variables desde .env si el archivo existe (solo desarrollo local)
DotNetEnv.Env.Load();

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
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts =>
    {
        // Serializar enums como strings en la API
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Evitar referencias circulares
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// --- REST API ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LogisticaBroker API",
        Version = "v1",
        Description = "API REST para el sistema de gestión logística."
    });

    // Esquema de seguridad Bearer (cookie de Identity)
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = ".AspNetCore.Identity.Application",
        Description = "Cookie de sesión generada por ASP.NET Core Identity"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "cookieAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LogisticaBroker API v1");
        c.RoutePrefix = "api/docs";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
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

// Rutas API (los ApiControllers se registran automáticamente por atributos)
app.MapControllers();

app.MapRazorPages();

app.Run();