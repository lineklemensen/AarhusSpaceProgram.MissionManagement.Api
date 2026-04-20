using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AarhusSpaceProgram.MissionManagement.Api.Infrastructure
{
    public class SeedData
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AspUser> _userManager;
        private readonly MissionManagementDbContext _context;

        public SeedData(
            RoleManager<IdentityRole> roleManager,
            UserManager<AspUser> userManager,
            MissionManagementDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();

            await SeedAstronautsAsync();
            await SeedScientistsAsync();
            await SeedManagersAsync();

            await SeedCelestialBodies();
            await SeedLaunchpadsAsync();
            await SeedRocketsAsync();
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Astronaut", "Scientist", "Manager" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedAstronautsAsync()
        {
            if (await _context.Astronauts.AnyAsync())
                return;

            // Seed astronauts
            var astronautSeeds = new[]
            {
                new
                {
                    Name = "Neil Legstrong",
                    Rank = AstronautRank.Astronaut,
                    Paygrade = "2-A",
                    HoursInSimulation = 500,
                    HoursInSpace = 200
                },

                new
                {
                    Name = "Buzz Lightyear",
                    Rank = AstronautRank.Pilot,
                    Paygrade = "3-A",
                    HoursInSimulation = 600,
                    HoursInSpace = 150
                },

                new
                {
                    Name = "Sally Ride",
                    Rank = AstronautRank.MissionSpecialist,
                    Paygrade = "3-A",
                    HoursInSimulation = 700,
                    HoursInSpace = 200
                }
            };

            int startNumber = 100000;
            var astronauts = new List<Astronaut>();

            for (int i = 0; i < astronautSeeds.Length; i++)
            {
                var seed = astronautSeeds[i];
                var id = $"A{startNumber + i}";
                var email = $"{id}@asp.com";
                var password = "A-test123!";

                // Create user
                var user = new AspUser
                {
                    Id = id,
                    UserName = id,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                    throw new Exception($"Could not create user {id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                // Assign role
                await _userManager.AddToRoleAsync(user, "Astronaut");

                // Create astronaut and link to user
                var astronaut = new Astronaut
                {
                    Id = id,
                    Name = seed.Name,
                    Rank = seed.Rank,
                    Paygrade = seed.Paygrade,
                    HoursInSimulation = seed.HoursInSimulation,
                    HoursInSpace = seed.HoursInSpace,
                    User = user
                };

                astronauts.Add(astronaut);
            }

            _context.Astronauts.AddRange(astronauts);
            await _context.SaveChangesAsync();
        }

        private async Task SeedScientistsAsync()
        {
            if (await _context.Scientists.AnyAsync())
                return;

            // Seed scientists
            var scientistSeeds = new[]
            {
                new
                {
                    Name = "Howard Wolowitz",
                    Title = "Aerospace Engineer",
                    Specialty = "Propulsion Systems",
                    HireDate = new DateTime(2010, 5, 1)
                },

                new
                {
                    Name = "Werner von Schwartz",
                    Title = "Astrophysicist",
                    Specialty = "Planetary Science",
                    HireDate = new DateTime(2012, 8, 15)
                }
            };

            int startNumber = 100000;
            var scientists = new List<Scientist>();

            for (int i = 0; i < scientistSeeds.Length; i++)
            {
                var seed = scientistSeeds[i];
                var id = $"S{startNumber + i}";
                var email = $"{id}@asp.com";
                var password = "S-test123!";

                // Create user
                var user = new AspUser
                {
                    Id = id,
                    UserName = id,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                    throw new Exception($"Could not create user {id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                // Assign role 
                await _userManager.AddToRoleAsync(user, "Scientist");

                // Create scientist and link to user
                var scientist = new Scientist
                {
                    Id = id,
                    Name = seed.Name,
                    Title = seed.Title,
                    Specialty = seed.Specialty,
                    HireDate = seed.HireDate,
                    User = user
                };

                scientists.Add(scientist);
            }

            _context.Scientists.AddRange(scientists);
            await _context.SaveChangesAsync();
        }

        private async Task SeedManagersAsync()
        {
            if (await _context.Managers.AnyAsync())
                return;

            var managerSeeds = new[]
            {
                new
                {
                    Name = "Dean Krantz"
                },

                new
                {
                    Name = "Ellen Cooper"
                }
            };

            int startNumber = 100000;
            var managers = new List<Manager>();

            for (int i = 0; i < managerSeeds.Length; i++)
            {
                var seed = managerSeeds[i];
                var id = $"M{startNumber + i}";
                var email = $"{id}@asp.com";
                var password = "M-test123!";

                // Create user
                var user = new AspUser
                {
                    Id = id,
                    UserName = id,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                    throw new Exception($"Could not create user {id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                // Assign role
                await _userManager.AddToRoleAsync(user, "Manager");

                // Create manager and link to user
                var manager = new Manager
                {
                    Id = id,
                    Name = seed.Name,
                    User = user
                };

                managers.Add(manager);
            }

            _context.Managers.AddRange(managers);
            await _context.SaveChangesAsync();
        }
 
        /* TODO: Add seeding for missions after seeding more basic entities
        private async Task SeedMissionsAsync()
        {

        }
        */

        private async Task SeedCelestialBodies()
        {
            if (await _context.CelestialBodies.AnyAsync())
                return;

            var earth = new CelestialBody
            {
                Name = "Earth",
                BodyType = CelestialBodyType.Planet,
                PlanetClass = PlanetClass.Rocky,
                DistanceValueToParentAU = 1.0
            };

            var mars = new CelestialBody
            {
                Name = "Mars",
                BodyType = CelestialBodyType.Planet,
                PlanetClass = PlanetClass.Rocky,
                DistanceValueToParentAU = 1.524
             };

            _context.CelestialBodies.AddRange(earth, mars);
            await _context.SaveChangesAsync();

            var earthId = earth.Id;
            
            var moon = new CelestialBody
            {
                Name = "Moon",
                BodyType = CelestialBodyType.Moon,
                DistanceValueToParentAU = 0.00257,
                ParentId = earthId
             };

             _context.CelestialBodies.Add(moon);
             await _context.SaveChangesAsync();
        }
    
        private async Task SeedLaunchpadsAsync()
        {
                if (await _context.Launchpads.AnyAsync())
                    return;

            var kennedy = new Launchpad
            {
                PadCode = "LC-39A",
                Location = "Kennedy Space Center, Florida, USA",
                Status = LaunchpadStatus.Operational,
                MaxSupportedWeightKg = 63800
            };

            var capeCanaveral = new Launchpad
            {
                PadCode = "SLC-40",
                Location = "Cape Canaveral Space Force Station, Florida, USA",
                Status = LaunchpadStatus.Operational,
                MaxSupportedWeightKg = 22800
            };
        }
    
        private async Task SeedRocketsAsync()
        {
                if (await _context.Rockets.AnyAsync())
                    return;

            var atlas = new Rocket
            {
                Name = "Atlas V 541",
                PayloadCapacityKg = 17440,
                CrewCapacity = 0,
                NumberOfStages = 2,
                FuelCapacityKg = 284000,
                WeightKg = 49000
            };

            var slsblock = new Rocket
            {
                Name = "Space Launch System Block 1",
                PayloadCapacityKg = 95000,
                CrewCapacity = 4,
                NumberOfStages = 2,
                FuelCapacityKg = 733000,
                WeightKg = 130000
            };
        }
    }
}
