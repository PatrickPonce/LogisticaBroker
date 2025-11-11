using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Models;

namespace LogisticaBroker.Data
{
    // Recuerda: IdentityDbContext<ApplicationUser> para usar tu usuario personalizado
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Registramos las nuevas tablas
        public DbSet<Client> Clients { get; set; }
        public DbSet<Dispatch> Dispatches { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DispatchTimeline> DispatchTimelines { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<DispatchCost> DispatchCosts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuraciones adicionales (opcional pero recomendado)
            // Por ejemplo, asegurar que el RUC sea único
            builder.Entity<Client>()
                .HasIndex(c => c.RUC)
                .IsUnique();

             builder.Entity<Dispatch>()
                .HasIndex(d => d.DispatchNumber)
                .IsUnique();
        }
    }
}