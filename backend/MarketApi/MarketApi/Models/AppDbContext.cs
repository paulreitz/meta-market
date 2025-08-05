using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MarketApi.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFile> ProductFiles { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Images)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                        v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions())
                    );

                entity.Property(e => e.Currency)
                    .HasConversion<string>(
                        v => v.ToString(),
                        v => Enum.Parse<Currency>(v)
                     )
                    .HasMaxLength(10);

            });
        }
    }
}

