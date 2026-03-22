namespace AarhusSpaceProgram.MissionManagement.Api.DTO
{
    public sealed record RocketListItemDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int PayloadCapacityKg { get; set; }

        public int CrewCapacity { get; set; }

        public int NumberOfStages { get; set; }

        public int FuelCapacityKg { get; set; }

        public int WeightKg { get; set; }
    }

    public sealed class UpdateRocketDTO
    {
        public string? Name { get; set; } = null!;

        public int? PayloadCapacityKg { get; set; }

        public int? CrewCapacity { get; set; }

        public int? NumberOfStages { get; set; }

        public int? FuelCapacityKg { get; set; }

        public int? WeightKg { get; set; }
    }
}
