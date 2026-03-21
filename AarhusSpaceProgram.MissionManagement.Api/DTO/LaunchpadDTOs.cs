namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed record LaunchpadListItemDTO
    {
        public int Id { get; init; }

        public string PadCode { get; init; } = null!;

        public string Location { get; init; } = null!;

        public string Status { get; init; } = null!;

        public int MaxSupportedWeightKg { get; init; }
    }
}
