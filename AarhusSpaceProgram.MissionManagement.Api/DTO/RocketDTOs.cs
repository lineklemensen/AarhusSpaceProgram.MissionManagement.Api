namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed record RocketListItemDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = null!;

        public int PayloadCapacityKg { get; init; }

        public int CrewCapacity { get; init; }

        public int NumberOfStages { get; init; }

        public int FuelCapacityKg { get; init; }

        public int WeightKg { get; init; }
    }
}
