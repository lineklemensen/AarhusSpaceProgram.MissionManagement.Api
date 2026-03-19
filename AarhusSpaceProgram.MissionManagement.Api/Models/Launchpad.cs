using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("Launchpads")]
    public class Launchpad
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = null!;

        [Required]
        public LaunchpadStatus Status { get; set; }

        [Required]
        public int MaxSupportedWeight { get; set; }

    }
}
