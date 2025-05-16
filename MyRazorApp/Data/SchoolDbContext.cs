using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyRazorApp.Models;

namespace MyRazorApp.Data
{
    public class SchoolDbContext : IdentityDbContext<AppIdentityUser>
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
            : base(options)
        {
        }

        public DbSet<Class> Classes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ClassInformationModel> ClassInformationModels { get; set; }
        public DbSet<ClassInformationTable> ClassInformationTables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Identity key konfigürasyonları
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey });

            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
                .HasKey(r => new { r.UserId, r.RoleId });

            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>()
                .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            // Opsiyonel: diğer tabloların konfigürasyonu eklenebilir
        }
    }
}
