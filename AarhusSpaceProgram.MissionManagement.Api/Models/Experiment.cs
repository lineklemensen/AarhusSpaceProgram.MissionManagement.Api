using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("Experiments")]
    public class Experiment
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateOnly CreationDate { get; set; }

        // Navigation properties
        public ICollection<ExperimentScientistAssignment> ScientistAssignments { get; set; } = new List<ExperimentScientistAssignment>();

        public ICollection<ExperimentAstronautAssignment> AstronautAssignments { get; set; } = new List<ExperimentAstronautAssignment>();

        public ICollection<ExperimentsOnMissions> Missions { get; set; } = new List<ExperimentsOnMissions>();
    }
}
