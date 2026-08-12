using WordGame.Domain.Entities;

namespace WordGame.Application.Abstractions;

/// <summary>
/// Stores rooms for the lifetime of the process. Rooms never persist across a restart —
/// see the project notes on this being an intentional, accepted trade-off.
/// </summary>
public interface IRoomRepository
{
    void Add(Room room);
    bool TryGet(string code, out Room? room);
    bool Contains(string code);
}
