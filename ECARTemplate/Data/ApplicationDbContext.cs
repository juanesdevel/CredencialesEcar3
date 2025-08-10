using Microsoft.EntityFrameworkCore;
using ECARTemplate.Models; // Asegúrate de que tus modelos estén en este namespace

// Ya no necesitamos la referencia a controladores aquí, porque SpResult
// no será un DbSet y solo se usará en el controlador.
// using ECARTemplate.Controllers;

namespace ECARTemplate.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tus DbSets existentes para tus modelos de base de datos
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Credencial> Credenciales { get; set; }

        // --- ¡IMPORTANTE! Este DbSet NO debe estar presente para tu versión de EF Core. ---
        // public DbSet<SpResult> SpResults { get; set; } // <<-- ¡ELIMINA ESTA LÍNEA!

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- ¡IMPORTANTE! Asegúrate de que NINGUNA configuración para SpResult esté aquí. ---
            // modelBuilder.Query<SpResult>();      // <<-- ¡ELIMINA ESTA LÍNEA!
            // modelBuilder.Entity<SpResult>().HasNoKey(); // <<-- ¡ELIMINA ESTA LÍNEA!

            // Si tienes otras configuraciones de modelo para tus entidades existentes (UsuarioTI, Empleado, Equipo, Credencial),
            // asegúrate de que permanezcan aquí.
        }
    }
}