using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public class MissionManagementDTOs
    {
        // Overview
        public sealed record MissionOverviewDTO
        {
            public int Id { get; init; }

            public string Name { get; init; } = null!;

            public string ManagerName { get; init; } = null!;

            public string Status { get; init; } = null!;

            public DateOnly? LaunchDate { get; init; }

            public string? RocketModel { get; init; } = null!;

            public string? LaunchpadLocation { get; init; } = null!;

            public string? TargetCelestialBody { get; init; } = null!;
        }

        // Details
        public sealed record MissionDetailsDTO
        {
            public string Name { get; init; } = null!;

            public string ManagerName { get; init; } = null!;

            public string Status { get; init; } = null!;

            public string TargetCelestialBody { get; init; } = null!;

            public List<AstronautListItemDTO> Astronauts { get; init; } = new();

            public List<ScientistListItemDTO> Scientists { get; init; } = new();

            public DateOnly? LaunchDate { get; init; }

            public string? RocketModel { get; init; } = null!;

            public string? LaunchpadCode { get; init; } = null!;

            public string? LaunchpadLocation { get; init; } = null!;
        }

        // Astronaut
        public sealed class AstronautAssignmentItemDTO
        {
            [Required]
            public string AstronautId { get; set; } = null!;
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

        // Scientist
        public sealed class ScientistAssignmentItemDTO
        {
            [Required]
            public string ScientistId { get; set; } = null!;
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

        // Celestial Body
        public sealed class CelestialBodyAssignmentDTO
        {
            [Required]
            public int MissionId { get; set; }

            [Required]
            public int? CelestialBodyId { get; set; }
        }

        // Launchpad
        public sealed class LaunchpadAssignmentDTO
        {
            [Required]
            public int MissionId { get; set; }

            [Required]
            public int? LaunchpadId { get; set; }
        }

        // Manager
        public sealed class ManagerAssignmentDTO
        {
            [Required]
            public int MissionId { get; set; }

            [Required]
            public string? ManagerId { get; set; } = null!;
        }

        // Rocket
        public sealed class RocketAssignmentDTO
        {
            [Required]
            public int MissionId { get; set; }

            [Required]
            public int? RocketId { get; set; }
        }
    }
}
