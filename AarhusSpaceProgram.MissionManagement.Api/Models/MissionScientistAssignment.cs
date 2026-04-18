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
        public string ScientistId { get; set; } = null!;
        public Scientist Scientist { get; set; } = null!;
    }
}
