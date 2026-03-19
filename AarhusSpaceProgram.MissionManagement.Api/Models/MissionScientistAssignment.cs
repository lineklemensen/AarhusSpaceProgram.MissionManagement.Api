using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Tags("MissionScientistAssignments")]
    public class MissionScientistAssignment
    {
        [Key]
        [Required]
        public int MissionId { get; set; }
        public Mission Mission { get; set; } = null!;

        [Key]
        [Required]
        public int ScientistId { get; set; }
        public Scientist Scientist { get; set; } = null!;
    }
}
