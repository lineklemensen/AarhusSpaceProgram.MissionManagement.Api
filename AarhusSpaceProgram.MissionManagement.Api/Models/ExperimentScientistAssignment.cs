using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("ExperimentScientistAssignments")]
    public class ExperimentScientistAssignment
    {
        [Key]
        [Required]
        public int ExperimentId { get; set; }

        public Experiment Experiment { get; set; } = null!;

        [Key]
        [Required]
        public string ScientistId { get; set; } = null!;

        public Scientist Scientist { get; set; } = null!;
    }
}
