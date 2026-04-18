using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed class CreateScientistDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Specialty { get; set; } = null!;

        [Required]
        public DateTime HireDate { get; set; }
    }

    public sealed record ScientistListItemDTO
    {
        public string Id { get; init; } = null!;

        public string Name { get; init; } = null!;

        public string Title { get; init; } = null!;

        public string Specialty { get; init; } = null!;

        public DateTime HireDate { get; init; }
    }

    public sealed class UpdateScientistDTO
    {
        public string? Name { get; set; } = null!;
        public string? Title { get; set; } = null!;
        public string? Specialty { get; set; } = null!;
    }
}
