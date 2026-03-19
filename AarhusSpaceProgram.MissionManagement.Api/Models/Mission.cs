using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("Missions")]
    public class Mission
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public DateOnly? LaunchDate { get; set; }

        public int? DurationHours { get; set; }

        [Required]
        public MissionStatus Status { get; set; }

        public MissionType Type { get; set; }
    }
}
