using System.Collections.Concurrent;
using WordGame.Application.Abstractions;
using WordGame.Domain.Entities;

namespace WordGame.Infrastructure.Rooms;

/// <summary>
/// Rooms live only in process memory for the lifetime of the app — deliberately not
/// persisted, so a server restart drops all in-flight rooms. See project notes.
/// Registered as a singleton so state survives across requests/hub connections.
/// </summary>
public class InMemoryRoomRepository : IRoomRepository
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();

    public void Add(Room room) => _rooms[room.Code] = room;

    public bool TryGet(string code, out Room? room) => _rooms.TryGetValue(code.ToUpperInvariant(), out room);

    public bool Contains(string code) => _rooms.ContainsKey(code);
}
