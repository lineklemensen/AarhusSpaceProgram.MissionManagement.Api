using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed class CreateRocketDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
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

    public sealed record RocketListItemDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int PayloadCapacityKg { get; set; }

        public int CrewCapacity { get; set; }

        public int NumberOfStages { get; set; }

        public int FuelCapacityKg { get; set; }

        public int WeightKg { get; set; }
    }

    public sealed class UpdateRocketDTO
    {
        public string? Name { get; set; } = null!;

        public int? PayloadCapacityKg { get; set; }

        public int? CrewCapacity { get; set; }

        public int? NumberOfStages { get; set; }

        public int? FuelCapacityKg { get; set; }

        public int? WeightKg { get; set; }
    }
}
