namespace WordGame.Domain.Entities;

public class Player
{
    public required Guid PlayerId { get; init; }
    public required string Nickname { get; set; }
    public string? ConnectionId { get; set; }
}
