using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("Managers")]
    public class Manager
    {
        [Key]
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        // Navigation properties to missions managed by this manager
        public ICollection<Mission> Missions { get; set; } = new List<Mission>();

        // Navigation property to the associated user account
        public AspUser? User { get; set; }
    }
}
