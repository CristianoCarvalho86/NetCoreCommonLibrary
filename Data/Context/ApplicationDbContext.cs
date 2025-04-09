using NetCoreCommonLibrary.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace NetCoreCommonLibrary.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ReportHeader> ReportHeaders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ReportHeader>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Client).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Project).IsRequired().HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

                entity.Property(e => e.Elevation).HasPrecision(10, 2);
                entity.Property(e => e.WaterLevel).HasPrecision(10, 2);
                entity.Property(e => e.HammerWeight).HasPrecision(10, 2);
            });
        }
    }
} 