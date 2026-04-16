using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("Scientists")]
    public class Scientist
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Specialty { get; set; } = null!;

        [Required]
        public DateTime HireDate { get; set; }

        // Navigation properties
        public int? UserId { get; set; }
        public AspUser? User { get; set; }
        public ICollection<MissionScientistAssignment> MissionAssignments { get; set; } = new List<MissionScientistAssignment>();
    }
}
