using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    public class MissionManagementDbContext : IdentityDbContext<AspUser>
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
            modelBuilder.Entity<Astronaut>(entity =>
            {
                entity.HasOne(a => a.User)
                    .WithOne(u => u.Astronaut)
                    .HasForeignKey<Astronaut>(a => a.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(a => a.Rank).HasConversion<string>();
            });

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
            modelBuilder.Entity<Launchpad>(entity =>
            {
                entity.HasIndex(l => l.PadCode).IsUnique();
                entity.Property(l => l.Status).HasConversion<string>();
            });

            // Manager
            modelBuilder.Entity<Manager>(entity =>
            {
                entity.HasOne(m => m.User)
                    .WithOne(u => u.Manager)
                    .HasForeignKey<Manager>(m => m.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Mission
            modelBuilder.Entity<Mission>(entity =>
            {
                entity.HasOne(m => m.Manager)
                    .WithMany(m => m.Missions)
                    .HasForeignKey(m => m.ManagerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(m => m.Rocket)
                    .WithMany(m => m.Missions)
                    .HasForeignKey(m => m.RocketId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(m => m.Launchpad)
                    .WithMany(m => m.Missions)
                    .HasForeignKey(m => m.LaunchpadId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(m => m.TargetBody)
                    .WithMany(m => m.TargetedByMissions)
                    .HasForeignKey(m => m.CelestialBodyId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(m => m.Status).HasConversion<string>();
                entity.Property(m => m.Type).HasConversion<string>();
            });

            // Scientist
            modelBuilder.Entity<Scientist>(entity =>
            {
                entity.HasOne(s => s.User)
                    .WithOne(u => u.Scientist)
                    .HasForeignKey<Scientist>(s => s.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Junction entities
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

            // ExperimentScientistAssignment
            modelBuilder.Entity<ExperimentScientistAssignment>(entity =>
            {
                entity.HasKey(es => new { es.ExperimentId, es.ScientistId });

                entity.HasOne(es => es.Experiment)
                    .WithMany(e => e.ScientistAssignments)
                    .HasForeignKey(es => es.ExperimentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(es => es.Scientist)
                    .WithMany(s => s.ExperimentAssignments)
                    .HasForeignKey(es => es.ScientistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ExperimentAstronautAssignment
            modelBuilder.Entity<ExperimentAstronautAssignment>(entity =>
            {
                entity.HasKey(ea => new { ea.ExperimentId, ea.AstronautId });

                entity.HasOne(ea => ea.Experiment)
                    .WithMany(e => e.AstronautAssignments)
                    .HasForeignKey(ea => ea.ExperimentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ea => ea.Astronaut)
                    .WithMany(a => a.ExperimentAssignments)
                    .HasForeignKey(ea => ea.AstronautId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //ExperimentsOnMissions
            modelBuilder.Entity<ExperimentsOnMissions>(entity =>
            {
                entity.HasKey(em => new { em.MissionId, em.ExperimentId });

                entity.HasOne(em => em.Mission)
                    .WithMany(m => m.Experiments)
                    .HasForeignKey(em => em.MissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(em => em.Experiment)
                    .WithMany(e => e.Missions)
                    .HasForeignKey(em => em.ExperimentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // DbSets for primary entities
        public DbSet<Astronaut> Astronauts => Set<Astronaut>();
        public DbSet<CelestialBody> CelestialBodies => Set<CelestialBody>();
        public DbSet<Launchpad> Launchpads => Set<Launchpad>();
        public DbSet<Manager> Managers => Set<Manager>();
        public DbSet<Mission> Missions => Set<Mission>();
        public DbSet<Rocket> Rockets => Set<Rocket>();
        public DbSet<Scientist> Scientists => Set<Scientist>();

        // DbSets for junction entities
        public DbSet<MissionAstronautAssignment> MissionAstronautAssignments => Set<MissionAstronautAssignment>();
        public DbSet<MissionScientistAssignment> MissionScientistAssignments => Set<MissionScientistAssignment>();
    }
}
