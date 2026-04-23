using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("ExperimentsOnMissions")]
    public class ExperimentsOnMissions
    {
        [Key]
        [Required]
        public int MissionId { get; set; }

        public Mission Mission { get; set; } = null!;

        [Key]
        [Required]
        public int ExperimentId { get; set; }

        public Experiment Experiment { get; set; } = null!;
    }
}
