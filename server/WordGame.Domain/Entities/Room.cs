namespace WordGame.Domain.Entities;

/// <summary>
/// Aggregate root for a game room. Rooms are transient — they only ever live
/// in memory for the lifetime of the process (see IRoomRepository).
/// </summary>
public class Room
{
    private readonly Dictionary<Guid, Player> _players = new();
    private readonly Dictionary<Guid, WordAssignment> _lastAssignments = new();

    public required string Code { get; init; }
    public required Guid HostPlayerId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public IReadOnlyCollection<Player> Players => _players.Values;

    public bool IsHost(Guid playerId) => playerId == HostPlayerId;

    /// <summary>Adds a new player, or re-attaches an existing one under a new connection (e.g. after a reconnect).</summary>
    public void AddOrUpdatePlayer(Guid playerId, string nickname, string connectionId)
    {
        _players[playerId] = new Player
        {
            PlayerId = playerId,
            Nickname = nickname,
            ConnectionId = connectionId,
        };
    }

    /// <summary>
    /// Removes a player only if the given connection is still the one currently on file for them.
    /// A stale connection disconnecting after a newer one has taken over (e.g. a fast page refresh,
    /// or a server-restart reconnect) must not evict the still-connected player.
    /// </summary>
    public void RemovePlayerIfConnectionMatches(Guid playerId, string connectionId)
    {
        if (_players.TryGetValue(playerId, out var player) && player.ConnectionId == connectionId)
        {
            _players.Remove(playerId);
        }
    }

    public WordAssignment? GetLastAssignment(Guid playerId) =>
        _lastAssignments.GetValueOrDefault(playerId);

    /// <summary>
    /// Distributes a new word to every player currently in the room: ~30% (minimum one,
    /// unless there's only a single player) receive only the hint, the rest get the full word.
    /// </summary>
    public IReadOnlyDictionary<Guid, WordAssignment> AssignNewWord(Word word)
    {
        var players = _players.Values.ToList();
        _lastAssignments.Clear();

        if (players.Count == 0)
        {
            return _lastAssignments;
        }

        var hintCount = ComputeHintCount(players.Count);
        var hintPlayerIds = players
            .OrderBy(_ => Random.Shared.Next())
            .Take(hintCount)
            .Select(p => p.PlayerId)
            .ToHashSet();

        foreach (var player in players)
        {
            var isHintOnly = hintPlayerIds.Contains(player.PlayerId);
            var assignment = new WordAssignment
            {
                WordId = word.Id,
                WordText = word.Text,
                IsHintOnly = isHintOnly,
                Content = isHintOnly ? word.Hint : word.Text,
            };
            _lastAssignments[player.PlayerId] = assignment;
        }

        return _lastAssignments;
    }

    /// <summary>How many of <paramref name="playerCount"/> players should receive only the hint.</summary>
    public static int ComputeHintCount(int playerCount)
    {
        if (playerCount <= 1)
        {
            return 0; // solo testing: only the word makes sense
        }

        var hintCount = Math.Max(1, (int)Math.Round(playerCount * 0.3));
        return Math.Min(hintCount, playerCount - 1);
    }
}
