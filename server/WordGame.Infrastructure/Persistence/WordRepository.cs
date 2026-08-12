using Microsoft.EntityFrameworkCore;
using WordGame.Application.Abstractions;
using WordGame.Domain.Entities;

namespace WordGame.Infrastructure.Persistence;

public class WordRepository(IDbContextFactory<GameDbContext> dbContextFactory) : IWordRepository
{
    public async Task<Word?> GetRandomActiveWordAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var activeCount = await db.Words.CountAsync(w => w.IsActive, cancellationToken);
        if (activeCount == 0)
        {
            return null;
        }

        var skip = Random.Shared.Next(activeCount);
        return await db.Words.Where(w => w.IsActive).Skip(skip).Take(1).FirstAsync(cancellationToken);
    }
}
