using Microsoft.EntityFrameworkCore;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    public class MissionManagementDbContext : DbContext
    {
        public MissionManagementDbContext(
            DbContextOptions<MissionManagementDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Mission>(entity =>
            {
                entity.Property(m => m.Status).HasConversion<string>();
                entity.Property(m => m.Type).HasConversion<string>();
            });

            modelBuilder.Entity<Astronaut>()
                .Property(a => a.Rank)
                .HasConversion<string>();

            modelBuilder.Entity<Launchpad>()
                .Property(l => l.Status)
                .HasConversion<string>();

            modelBuilder.Entity<CelestialBody>(entity =>
            {
                entity.Property(cb => cb.BodyType).HasConversion<string>();
                entity.Property(cb => cb.PlanetClass).HasConversion<string>();

                entity.HasOne(cb => cb.Parent)
                    .WithMany(cb => cb.Children)
                    .HasForeignKey(cb => cb.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
