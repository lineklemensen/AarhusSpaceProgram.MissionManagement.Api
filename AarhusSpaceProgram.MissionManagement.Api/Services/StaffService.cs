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

        public async Task<(string Id, string Email, string TemporaryPassword)> CreateUser(CreateUserDTO dto)
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
            IEnumerable<string> existingIds = _userManager.Users
                .Where(u => u.Id.StartsWith(prefix))
                .Select(u => u.Id);

            // Generate new staff ID
            string newId = StaffIdGenerator.GenerateStaffId(prefix, existingIds);

            // Generate organization email
            string email = $"{newId}@{dto.EmailDomain}";

            // Generate temporary password
            string tempPassword = GenerateTemporaryPassword();

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

            // Assign role to user
            await _userManager.AddToRoleAsync(user, dto.Role);

            // Return new staff ID, email, and temporary password
            return (newId, email, tempPassword);
        }

        public string GenerateTemporaryPassword()
        {
            const int passwordLength = 8;
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digit = "0123456789";
            const string special = "!@#$%^&*()";
            const string all = upper + lower + digit + special;
            var random = new Random();

            // Use one of each char type in the password
            var passwordChars = new char[passwordLength];
            passwordChars[0] = upper[random.Next(upper.Length)];
            passwordChars[1] = lower[random.Next(lower.Length)];
            passwordChars[2] = digit[random.Next(digit.Length)];
            passwordChars[3] = special[random.Next(special.Length)];

            // Fill remaining spots with random chars from all sets
            for (int i = 4; i < passwordLength; i++)
                passwordChars[i] = all[random.Next(all.Length)];
            
            // Shuffle
            return new string(passwordChars.OrderBy(_ => random.Next()).ToArray());
        }
    }
}
