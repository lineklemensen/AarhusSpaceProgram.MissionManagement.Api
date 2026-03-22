using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed class CreateManagerDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = null!;
    }

    public sealed record ManagerListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;
    }

    public sealed class UpdateManagerDTO
    {
        public string Name { get; set; } = null!;
    }
}
