using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("Rockets")]
    public class Rocket
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        public int PayloadCapacityKg { get; set; }

        [Required]
        public int CrewCapacity { get; set; }

        [Required]
        public int NumberOfStages { get; set; }

        [Required]
        public int FuelCapacityKg { get; set; }

        [Required]
        public int WeightKg { get; set; }
    }
}
