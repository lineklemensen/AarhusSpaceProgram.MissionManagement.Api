namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed record MissionListDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public string Status { get; init; } = null!;
    }
}
