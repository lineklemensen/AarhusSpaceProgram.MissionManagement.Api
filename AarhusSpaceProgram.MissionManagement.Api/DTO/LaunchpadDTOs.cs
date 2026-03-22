using System.ComponentModel.DataAnnotations;

namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed class CreateLaunchpadDTO
    {
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string PadCode { get; set; } = null!;

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Location { get; set; } = null!;

        [Required]
        public string Status { get; set; } = null!;

        [Required]
        public int MaxSupportedWeightKg { get; set; }
    }
    public sealed record LaunchpadListItemDTO
    {
        public int Id { get; set; }

        public string PadCode { get; set; } = null!;

        public string Location { get; set; } = null!;

        public string Status { get; set; } = null!;

        public int MaxSupportedWeightKg { get; set; }
    }

    public sealed class UpdateLaunchpadDTO
    {
        public string? PadCode { get; set; } = null!;
        
        public string? Location { get; set; } = null!;
        
        public string? Status { get; set; } = null!;
        
        public int? MaxSupportedWeightKg { get; set; }
    }

}
