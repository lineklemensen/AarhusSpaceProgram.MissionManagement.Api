namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed record ScientistListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public string Title { get; init; } = null!;

        public string Specialty { get; init; } = null!;

        public DateTime HireDate { get; init; }
    }
}
