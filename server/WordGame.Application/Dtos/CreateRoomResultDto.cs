namespace WordGame.Application.Dtos;

public record CreateRoomResultDto(string RoomCode, Guid HostPlayerId);
