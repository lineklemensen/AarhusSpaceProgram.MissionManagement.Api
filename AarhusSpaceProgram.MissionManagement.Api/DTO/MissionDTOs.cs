using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed class CreateMissionDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        public DateOnly? LaunchDate { get; set; }

        public int? DurationHours { get; set; }

        [Required]
        public string Status { get; set; } = null!;

        public string Type { get; set; } = null!;
    }

    public sealed record MissionSimpleListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public string Status { get; init; } = null!;
    }

    public sealed record MissionListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public DateOnly? LaunchDate { get; init; }

        public int? DurationHours { get; init; }

        public string Status { get; init; } = null!;

        public string Type { get; init; } = null!;
    }

    public sealed record MissionOverviewDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public string ManagerName { get; init; } = null!;

        public string Status { get; init; } = null!;

        public DateOnly? LaunchDate { get; init; }

        public string? RocketModel {  get; init; } = null!;

        public string? LaunchpadLocation { get; init; } = null!;

        public string? TargetCelestialBody { get; init; } = null!;
    }

    public sealed class UpdateMissionDTO
    {
        public string? Name { get; set; } = null!;

        public DateOnly? LaunchDate { get; set; }

        public int? DurationHours { get; set; }

        public string? Status { get; set; }

        public string? Type { get; set; }
    }

    public sealed class AstronautAssignmentItemDTO
    {
        [Required]
        public int AstronautId { get; set; }
    }

    public sealed class AssignAstronautsToMissionDTO
    {
        [Required]
        [MinLength(1)]
        public List<AstronautAssignmentItemDTO> Astronauts { get; set; } = new();
    }

    public sealed class RemoveAstronautsFromMissionDTO
    {
        [Required]
        [MinLength(1)]
        public List<AstronautAssignmentItemDTO> Astronauts { get; set; } = new();
    }

    public sealed class ScientistAssignmentItemDTO
    {
        [Required]
        public int ScientistId { get; set; }
    }

    public sealed class AssignScientistsToMissionDTO
    {
        [Required]
        [MinLength(1)]
        public List<ScientistAssignmentItemDTO> Scientists { get; set; } = new();
    }

    public sealed class RemoveScientistsFromMissionDTO
    {
        [Required]
        [MinLength(1)]
        public List<ScientistAssignmentItemDTO> Scientists { get; set; } = new();
    }
}