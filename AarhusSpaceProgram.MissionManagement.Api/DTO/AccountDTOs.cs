using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
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

    public class CreateStaffWithUserDTO
    {
        [Required]
        public string Role { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [StringLength(50)]
        public string EmailDomain { get; set; } = "asp.com";
    }
}
