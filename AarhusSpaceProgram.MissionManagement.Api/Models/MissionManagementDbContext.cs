using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;

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
            modelBuilder.Entity<Astronaut>(entity =>
            {
                entity.Property(a => a.Rank).HasConversion<string>();

                entity.HasData(
                    new Astronaut
                    {
                        Id = 1,
                        Name = "Neil Legstrong",
                        Rank = AstronautRank.Astronaut,
                        Paygrade = "2-A",
                        HoursInSimulation = 500,
                        HoursInSpace = 100
                    },
                    new Astronaut
                    {
                        Id = 2,
                        Name = "Buzz Lightyear",
                        Rank = AstronautRank.Pilot,
                        Paygrade = "3-A",
                        HoursInSimulation = 600,
                        HoursInSpace = 150
                    },
                    new Astronaut
                    {
                        Id = 3,
                        Name = "Sally Ride",
                        Rank = AstronautRank.MissionSpecialist,
                        Paygrade = "3-A",
                        HoursInSimulation = 700,
                        HoursInSpace = 200
                    }
                );

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

                entity.HasData(
                    new CelestialBody
                    {
                        Id = 1,
                        Name = "Earth",
                        BodyType = CelestialBodyType.Planet,
                        PlanetClass = PlanetClass.Rocky,
                        DistanceValueToParentAU = 1.0
                    },
                    new CelestialBody
                    {
                        Id = 2,
                        Name = "Moon",
                        BodyType = CelestialBodyType.Moon,
                        DistanceValueToParentAU = 0.00257,
                        ParentId = 1
                    },
                    new CelestialBody
                    {
                        Id = 3,
                        Name = "Mars",
                        BodyType = CelestialBodyType.Planet,
                        PlanetClass = PlanetClass.Rocky,
                        DistanceValueToParentAU = 1.524
                    }
                );
            });

            // Launchpad
            modelBuilder.Entity<Launchpad>(entity =>
            {
                entity.HasIndex(l => l.PadCode).IsUnique();
                entity.Property(l => l.Status).HasConversion<string>();

                entity.HasData(
                    new Launchpad
                    {
                        Id = 1,
                        PadCode = "LC-39A",
                        Location = "Kennedy Space Center, Florida, USA",
                        Status = LaunchpadStatus.Operational,
                        MaxSupportedWeightKg = 63800
                    },
                    new Launchpad
                    {
                        Id= 2,
                        PadCode = "SLC-41",
                        Location = "Cape Canaveral Space Force Station, Florida, USA",
                        Status = LaunchpadStatus.UnderMaintenance,
                        MaxSupportedWeightKg = 17440
                    }
                );
            });

            //Mission
            modelBuilder.Entity<Mission>(entity =>
            {
                entity.Property(m => m.Status).HasConversion<string>();
                entity.Property(m => m.Type).HasConversion<string>();

                entity.HasData(
                    new Mission
                    {
                        Id = 1,
                        Name = "Mars 2020",
                        LaunchDate = new DateOnly(2020, 7, 30),
                        DurationHours = 4872,
                        Status = MissionStatus.Completed,
                        Type = MissionType.Landing
                    },
                    new Mission
                    {
                        Id = 2,
                        Name = "Artemis I",
                        LaunchDate = new DateOnly(2022, 11, 16),
                        DurationHours = 613,
                        Status = MissionStatus.Completed,
                        Type = MissionType.Orbit
                    }
                );
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

        public DbSet<Astronaut> Astronauts => Set<Astronaut>();
        public DbSet<CelestialBody> CelestialBodies => Set<CelestialBody>();
        public DbSet<Launchpad> Launchpads => Set<Launchpad>();
        public DbSet<Manager> Managers => Set<Manager>();
        public DbSet<Mission> Missions => Set<Mission>();
        public DbSet<Rocket> Rockets => Set<Rocket>();
        public DbSet<Scientist> Scientists => Set<Scientist>();
        public DbSet<MissionAstronautAssignment> MissionAstronautAssignments => Set<MissionAstronautAssignment>();
        public DbSet<MissionScientistAssignment> MissionScientistAssignments => Set<MissionScientistAssignment>();
    }
}
