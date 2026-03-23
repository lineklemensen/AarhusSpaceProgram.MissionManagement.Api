using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public class MissionManagementDTOs
    {
        // Astronaut
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

        // Scientist
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

        // Celestial Body
        public sealed class CelestialBodyAssignmentDTO
        {
            [Required]
            public int MissionId { get; set; }

            [Required]
            public int? CelestialBodyId { get; set; }
        }

        // Manager
        public sealed class ManagerAssignmentDTO
        {
            [Required]
            public int MissionId { get; set; }

            [Required]
            public int? ManagerId { get; set; }
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
