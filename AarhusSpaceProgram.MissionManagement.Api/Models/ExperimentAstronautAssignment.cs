using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("ExperimentAstronautAssignments")]
    public class ExperimentAstronautAssignment
    {
        [Key]
        [Required]
        public int ExperimentId { get; set; }

        public Experiment Experiment { get; set; } = null!;

        [Key]
        [Required]
        public string AstronautId { get; set; } = null!;

        public Astronaut Astronaut { get; set; } = null!;
    }
}
