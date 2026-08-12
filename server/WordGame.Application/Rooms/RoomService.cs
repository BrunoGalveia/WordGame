using WordGame.Application.Abstractions;
using WordGame.Application.Dtos;
using WordGame.Domain.Entities;
using WordGame.Domain.Exceptions;

namespace WordGame.Application.Rooms;

public class RoomService(IRoomRepository roomRepository, IWordRepository wordRepository) : IRoomService
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I
    private static readonly Random CodeRandom = new();

    public CreateRoomResultDto CreateRoom()
    {
        string code;
        do
        {
            code = GenerateCode();
        } while (roomRepository.Contains(code));

        var hostPlayerId = Guid.NewGuid();
        var room = new Room { Code = code, HostPlayerId = hostPlayerId };
        roomRepository.Add(room);

        return new CreateRoomResultDto(room.Code, hostPlayerId);
    }

    public JoinRoomResultDto JoinRoom(string roomCode, string nickname, Guid playerId, string connectionId)
    {
        var room = GetRoomOrThrow(roomCode);

        room.AddOrUpdatePlayer(playerId, nickname, connectionId);

        var players = room.Players.Select(p => new PlayerSummaryDto(p.PlayerId, p.Nickname)).ToList();
        var assignment = ToDto(room.GetLastAssignment(playerId));

        return new JoinRoomResultDto(playerId, room.IsHost(playerId), players, assignment);
    }

    public void HandleDisconnect(string roomCode, Guid playerId, string connectionId)
    {
        if (roomRepository.TryGet(roomCode, out var room))
        {
            room!.RemovePlayerIfConnectionMatches(playerId, connectionId);
        }
    }

    public async Task<IReadOnlyDictionary<Guid, WordAssignmentDto>> RequestNewWordAsync(string roomCode, Guid requestingPlayerId)
    {
        var room = GetRoomOrThrow(roomCode);

        if (!room.IsHost(requestingPlayerId))
        {
            throw new NotHostException();
        }

        if (room.Players.Count == 0)
        {
            return new Dictionary<Guid, WordAssignmentDto>();
        }

        var word = await wordRepository.GetRandomActiveWordAsync()
            ?? throw new NoActiveWordsException();

        var assignments = room.AssignNewWord(word);

        return assignments.ToDictionary(kv => kv.Key, kv => ToDto(kv.Value)!);
    }

    private Room GetRoomOrThrow(string roomCode)
    {
        if (!roomRepository.TryGet(roomCode, out var room))
        {
            throw new RoomNotFoundException(roomCode);
        }

        return room!;
    }

    private static WordAssignmentDto? ToDto(WordAssignment? assignment) =>
        assignment is null
            ? null
            : new WordAssignmentDto(assignment.WordId, assignment.WordText, assignment.IsHintOnly, assignment.Content);

    private static string GenerateCode()
    {
        Span<char> buffer = stackalloc char[6];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = CodeAlphabet[CodeRandom.Next(CodeAlphabet.Length)];
        }
        return new string(buffer);
    }
}
