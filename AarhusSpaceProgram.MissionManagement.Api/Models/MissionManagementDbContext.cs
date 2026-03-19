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

            // Primary entities

            // Astronaut
            modelBuilder.Entity<Astronaut>()
                .Property(a => a.Rank)
                .HasConversion<string>();

            // CelestialBody
            modelBuilder.Entity<CelestialBody>(entity =>
            {
                entity.Property(cb => cb.BodyType).HasConversion<string>();
                entity.Property(cb => cb.PlanetClass).HasConversion<string>();

                // Configure self-referencing relationship for CelestialBody
                entity.HasOne(cb => cb.Parent)
                    .WithMany(cb => cb.Children)
                    .HasForeignKey(cb => cb.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Launchpad
            modelBuilder.Entity<Launchpad>()
                .Property(l => l.Status)
                .HasConversion<string>();

            //Mission
            modelBuilder.Entity<Mission>(entity =>
            {
                entity.Property(m => m.Status).HasConversion<string>();
                entity.Property(m => m.Type).HasConversion<string>();
            });

            // Junction entities

            // Composite keys
            // MissionAstronautAssignment
            modelBuilder.Entity<MissionAstronautAssignment>(entity =>
            {
                entity.HasKey(ma => new { ma.MissionId, ma.AstronautId });

                entity.HasOne(ma => ma.Mission)
                    .WithMany(m => m.AstronautAssignments)
                    .HasForeignKey(ma => ma.MissionId);

                entity.HasOne(ma => ma.Astronaut)
                    .WithMany(a => a.MissionAssignments)
                    .HasForeignKey(ma => ma.AstronautId);
            });

            // MissionScientistAssignment
            modelBuilder.Entity<MissionScientistAssignment>(entity =>
            {
                entity.HasKey(ms => new { ms.MissionId, ms.ScientistId });

                entity.HasOne(ms => ms.Mission)
                    .WithMany(m => m.ScientistAssignments)
                    .HasForeignKey(ms => ms.MissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ms => ms.Scientist)
                    .WithMany(s => s.MissionAssignments)
                    .HasForeignKey(ms => ms.ScientistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


        }
    }
}
