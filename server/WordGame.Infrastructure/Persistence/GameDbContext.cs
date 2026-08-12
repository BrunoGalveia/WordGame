using Microsoft.EntityFrameworkCore;
using WordGame.Domain.Entities;

namespace WordGame.Infrastructure.Persistence;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<Word> Words => Set<Word>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Word>(entity =>
        {
            entity.Property(w => w.Text).HasMaxLength(100).IsRequired();
            entity.Property(w => w.Hint).HasMaxLength(300).IsRequired();
            entity.Property(w => w.Category).HasMaxLength(100);
        });
    }
}
