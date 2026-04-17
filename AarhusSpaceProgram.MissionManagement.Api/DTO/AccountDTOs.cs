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
    }

    public class LoginDTO
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
