using WordGame.Application.Dtos;

namespace WordGame.Application.Rooms;

public interface IRoomService
{
    CreateRoomResultDto CreateRoom();

    JoinRoomResultDto JoinRoom(string roomCode, string nickname, Guid playerId, string connectionId);

    void HandleDisconnect(string roomCode, Guid playerId, string connectionId);

    Task<IReadOnlyDictionary<Guid, WordAssignmentDto>> RequestNewWordAsync(string roomCode, Guid requestingPlayerId);
}
