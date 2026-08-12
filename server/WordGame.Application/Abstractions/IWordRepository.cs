using WordGame.Domain.Entities;

namespace WordGame.Application.Abstractions;

public interface IWordRepository
{
    /// <summary>Returns a uniformly random active word, or null if none are active.</summary>
    Task<Word?> GetRandomActiveWordAsync(CancellationToken cancellationToken = default);
}
