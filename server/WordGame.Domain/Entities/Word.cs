namespace WordGame.Domain.Entities;

public class Word
{
    public Guid Id { get; set; }
    public required string Text { get; set; }
    public required string Hint { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}
