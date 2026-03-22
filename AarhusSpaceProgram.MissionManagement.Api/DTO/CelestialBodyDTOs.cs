using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed class CreateCelestialBodyDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;
        
        [Required]
        public string BodyType { get; set; } = null!;
        
        public string? PlanetClass { get; set; }
        
        [Required]
        public double DistanceValueToParentAU { get; set; }
        
        public int? ParentId { get; set; }
    }

    public sealed record CelestialBodyListItemDTO
    {
        public int Id { get; init; }
        
        public string Name { get; init; } = null!;
        
        public string BodyType { get; init; } = null!;

        public string? PlanetClass { get; init; } = null!;

        public double DistanceValueToParentAU { get; init; }

        public string? ParentName { get; init; } = null!;
    }

    public sealed class UpdateCelestialBodyDTO
    {
        public string Name { get; set; } = null!;
        
        public string? PlanetClass { get; set; }

        public int? ParentId { get; set; }
    }
}
