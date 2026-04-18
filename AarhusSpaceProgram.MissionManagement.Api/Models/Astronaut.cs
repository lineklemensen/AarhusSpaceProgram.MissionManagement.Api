using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("Astronauts")]
    public class Astronaut
    {
        [Key]
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        public AstronautRank Rank { get; set; }

        [Required]
        [MaxLength(20)]
        public string Paygrade { get; set; } = null!;

        [Required]
        public int HoursInSimulation { get; set; }

        [Required]
        public int HoursInSpace { get; set; }

        // Navigation properties
        public string? UserId { get; set; }
        public AspUser? User { get; set; }

        public ICollection<MissionAstronautAssignment> MissionAssignments { get; set; } = new List<MissionAstronautAssignment>();
    }
}
