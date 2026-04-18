using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    // Manually register a new user (for testing purposes or in case of emergencies)
    public class RegisterDTO
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public int StaffId { get; set; }

        [Required]
        public string Role { get; set; } = null!;
    }

    public class LoginDTO
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        public string? Password { get; set; }
    }

    // DTO for creating a new user automatically when a new staff member is added (without password, as it will be generated and sent to the user)
    public class CreateUserDTO
    {
        [Required]
        public string Role { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [StringLength(50)]
        public string EmailDomain { get; set; } = "asp.com";
    }
}
