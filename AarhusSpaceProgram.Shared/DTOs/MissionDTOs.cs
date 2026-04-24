namespace AarhusSpaceProgram.Shared.DTOs;

public sealed record MissionSimpleListItemDTO
{
    public int Id { get; init; }

    public string Name { get; init; } = null!;

    public string Status { get; init; } = null!;
}
