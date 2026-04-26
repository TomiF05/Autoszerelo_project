using Autoszerelo.models;
using Microsoft.EntityFrameworkCore;

namespace Autoszerelo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Ezek lesznek az adatbázis táblái
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Works> Works { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Works>()
        .HasOne(w => w.Customer)     // A munkának van egy ügyfele
        .WithMany(c => c.Works)      // Az ügyfélnek sok munkája van
        .HasForeignKey(w => w.CustomerId)
        .OnDelete(DeleteBehavior.Cascade); // <--- EZ A KULCS: Kaszkádolt törlés

            // Itt lehetne finomhangolni a táblákat, de alapból így is jó
        }
    }
}
