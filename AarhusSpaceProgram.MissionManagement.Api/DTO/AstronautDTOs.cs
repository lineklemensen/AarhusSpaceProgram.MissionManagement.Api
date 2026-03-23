using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed class CreateAstronautDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        [Required]
        public string Rank { get; set; } = null!;

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Paygrade { get; set; } = null!;

        public int HoursInSimulation { get; set; }

        public int HoursInSpace { get; set; }
    }

    public sealed record AstronautListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public string Rank { get; init; } = null!;

        public string Paygrade { get; init; } = null!;

        public int HoursInSimulation { get; init; }

        public int HoursInSpace { get; init; }
    }

    public record AstronautExperienceListItemDTO
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;

        public int HoursInSpace { get; init; }

        public int HoursInSimulation { get; init; }

        public int TotalExperience => HoursInSpace + HoursInSimulation;
    }

    public sealed class UpdateAstronautDTO
    {
        public string? Name { get; set; } = null!;
        public string? Rank { get; set; } = null!;
        public string? Paygrade { get; set; } = null!;
        public int? HoursInSimulation { get; set; }
        public int? HoursInSpace { get; set; }
    }
}
