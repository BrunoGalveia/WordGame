namespace WordGame.Application.Dtos;

public record WordAssignmentDto(Guid WordId, string WordText, bool IsHintOnly, string Content);
