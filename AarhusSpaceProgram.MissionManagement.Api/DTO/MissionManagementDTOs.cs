using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public class MissionManagementDTOs
    {
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
}
