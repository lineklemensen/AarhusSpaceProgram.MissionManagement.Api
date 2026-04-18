using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AarhusSpaceProgram.MissionManagement.Api.Services
{
    public class StaffService
    {
        private readonly MissionManagementDbContext _context;
        private readonly UserManager<AspUser> _userManager;

        public StaffService(
            MissionManagementDbContext context,
            UserManager<AspUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(string Id, string Email, string TemporaryPassword)> CreateStaffWithUser(CreateStaffWithUserDTO dto)
        {
            // Validate role
            string prefix = dto.Role switch
            {
                "Astronaut" => "A",
                "Scientist" => "S",
                "Manager" => "M",
                _ => throw new Exception($"Invalid role: {dto.Role}")
            };

            // Find existing IDs with the same prefix
            IEnumerable<string> existingIds = dto.Role switch
            {
                "Astronaut" => _context.Astronauts.Select(a => a.Id),
                "Scientist" => _context.Scientists.Select(s => s.Id),
                "Manager" => _context.Managers.Select(m => m.Id),
                _ => Enumerable.Empty<string>()
            };

            // Generate new staff ID
            string newId = StaffIdGenerator.GenerateStaffId(prefix, existingIds);

            // Generate organization email
            string email = $"{newId}@{dto.EmailDomain}";

            // Generate temporary password
            string tempPassword = GenerateTemporaryPassword();

            // Create staff member based on role
            dynamic staffMember = dto.Role switch
            {
                "Astronaut" => new Astronaut { Id = newId, Name = dto.Name },
                "Scientist" => new Scientist { Id = newId, Name = dto.Name },
                "Manager" => new Manager { Id = newId, Name = dto.Name },
                _ => throw new Exception($"Invalid role: {dto.Role}")
            };

            _context.Add(staffMember);

            // Create user account
            var user = new AspUser
            {
                Id = newId,
                UserName = newId,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, tempPassword);

            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Link user to staff member
            staffMember.User = user;
            await _context.SaveChangesAsync();

            // Return new staff ID, email, and temporary password
            return (newId, email, tempPassword);
        }

        private string GenerateTemporaryPassword()
        {
            const int passwordLength = 8;
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();

            return new string(Enumerable.Repeat(validChars, passwordLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
