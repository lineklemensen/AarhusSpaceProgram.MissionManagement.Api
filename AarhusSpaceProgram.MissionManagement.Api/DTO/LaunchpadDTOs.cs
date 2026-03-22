namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
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
