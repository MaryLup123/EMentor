using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using eduMentor.Models;

namespace eduMentor.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<eduMentor.Models.Certificado> Certificado { get; set; } = default!;
        public DbSet<eduMentor.Models.Curso> Curso { get; set; } = default!;
        public DbSet<eduMentor.Models.Inscripcion> Inscripcion { get; set; } = default!;
        public DbSet<eduMentor.Models.Modulo> Modulo { get; set; } = default!;
        public DbSet<eduMentor.Models.ProgresoModulo> ProgresoModulo { get; set; } = default!;
        public DbSet<eduMentor.Models.Role> Role { get; set; } = default!;
        public DbSet<eduMentor.Models.Usuario> Usuario { get; set; } = default!;
        public DbSet<eduMentor.Models.Contenido> Contenido { get; set; } = default!;
        public DbSet<eduMentor.Models.ProgresoContenido> ProgresoContenido { get; set; } = default!;
    }
}
