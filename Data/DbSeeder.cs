using Microsoft.AspNetCore.Identity;
using LogisticaBroker.Models;

namespace LogisticaBroker.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Administrador de usuarios y roles
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. Crear Roles si no existen
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            await roleManager.CreateAsync(new IdentityRole(Roles.Client));

            // 2. Crear usuario Admin por defecto si no existe
            var adminEmail = "admin@logisticabroker.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrador del Sistema",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                
                // Contraseña fuerte por defecto
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    // Asignar rol de Admin
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                }
            }
        }
    }

    // Clase estática para evitar "magic strings"
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Client = "Client";
    }
}