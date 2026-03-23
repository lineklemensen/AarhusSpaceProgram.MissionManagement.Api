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
        [MaxLength(20)]
        public string PadCode { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = null!;

        [Required]
        public LaunchpadStatus Status { get; set; }

        [Required]
        public int MaxSupportedWeightKg { get; set; }

        // Navigation properties to missions launched from this launchpad
        public ICollection<Mission> Missions { get; set; } = new List<Mission>();
    }
}
