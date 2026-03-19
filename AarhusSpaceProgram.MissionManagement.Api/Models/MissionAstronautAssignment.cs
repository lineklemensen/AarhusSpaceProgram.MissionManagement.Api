using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("MissionAstronautAssignments")]
    public class MissionAstronautAssignment
    {
        [Key]
        [Required]
        public int MissionId { get; set; }
        public Mission Mission { get; set; } = null!;

        [Key]
        [Required]
        public int AstronautId { get; set; }
        public Astronaut Astronaut { get; set; } = null!;
    }
}
