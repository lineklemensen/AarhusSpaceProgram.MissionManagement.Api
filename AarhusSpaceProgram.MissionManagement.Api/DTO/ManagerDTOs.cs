namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed record ManagerListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;
    }
}
