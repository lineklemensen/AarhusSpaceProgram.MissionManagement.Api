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

        // Navigation properties
        public ICollection<MissionAstronautAssignment> AstronautAssignments { get; set; } = new List<MissionAstronautAssignment>();
        public ICollection<MissionScientistAssignment> ScientistAssignments { get; set; } = new List<MissionScientistAssignment>();

        public string? ManagerId { get; set; }
        public Manager? Manager { get; set; }

        public int? RocketId { get; set; }
        public Rocket? Rocket { get; set; }

        public int? LaunchpadId { get; set; }
        public Launchpad? Launchpad { get; set; }

        public int? CelestialBodyId { get; set; }
        public CelestialBody? TargetBody { get; set; }

        public ICollection<ExperimentsOnMissions> Experiments { get; set; } = new List<ExperimentsOnMissions>();
    }
}
