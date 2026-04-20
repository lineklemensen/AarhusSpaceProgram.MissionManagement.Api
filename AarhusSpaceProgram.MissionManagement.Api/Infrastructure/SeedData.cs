using AarhusSpaceProgram.MissionManagement.Api.Models;
using Microsoft.AspNetCore.Identity;

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
        /*
        public async Task SeedAsync()
        {
            await SeedRolesAsync();

            //await SeedStaffAsync();
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

        /*
        private async Task SeedStaffAsync()
        {
            if (!await _context.Astronauts.AnyAsync())
            {
                _context.Astronauts.AddRange(
                    );
            }
        }
        */
    }
}
