using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    [Table("CelestialBodies")]
    public class CelestialBody
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        public CelestialBodyType BodyType { get; set; }

        public PlanetClass? PlanetClass { get; set; }

        [Required]
        public double DistanceValueToParentAU { get; set; }

        public int? ParentId { get; set; }

        // Navigation to parent celestial body
        public CelestialBody? Parent { get; set; }

        // Navigation to child celestial bodies
        public ICollection<CelestialBody> Children { get; set; } = new List<CelestialBody>();



    }
}
