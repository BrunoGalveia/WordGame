namespace WordGame.Domain.Entities;

/// <summary>
/// What a single player received for the current round: either the full word,
/// or — for the minority of players — only its hint.
/// </summary>
public class WordAssignment
{
    public required Guid WordId { get; init; }
    public required string WordText { get; init; }
    public required bool IsHintOnly { get; init; }
    public required string Content { get; init; } // the word itself, or the hint
}
