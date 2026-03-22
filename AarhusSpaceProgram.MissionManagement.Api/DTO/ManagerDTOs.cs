namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed class ManagerCreateDTO
    {
        public string Name { get; set; } = null!;
    }

    public sealed record ManagerListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;
    }

    public sealed class ManagerUpdateDTO
    {
        public string Name { get; set; } = null!;
    }
}
