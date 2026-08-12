namespace WordGame.Application.Dtos;

public record JoinRoomResultDto(
    Guid PlayerId,
    bool IsHost,
    IReadOnlyList<PlayerSummaryDto> Players,
    WordAssignmentDto? CurrentAssignment);
