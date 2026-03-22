namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
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

    public sealed class UpdateMissionDTO
    {
        public string? Name { get; set; } = null!;

        public DateOnly? LaunchDate { get; set; }
        
        public int? DurationHours { get; set; }
        
        public string? Status { get; set; }
        
        public string? Type { get; set; }
    }
}
