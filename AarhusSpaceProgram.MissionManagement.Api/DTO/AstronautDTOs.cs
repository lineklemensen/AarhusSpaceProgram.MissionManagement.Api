namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed record AstronautListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public string Rank { get; init; } = null!;

        public string Paygrade { get; init; } = null!;

        public int HoursInSimulation { get; init; }

        public int HoursInSpace { get; init; }
    }
}
