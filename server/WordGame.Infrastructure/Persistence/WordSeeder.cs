using Microsoft.EntityFrameworkCore;
using WordGame.Domain.Entities;

namespace WordGame.Infrastructure.Persistence;

public static class WordSeeder
{
    public static async Task SeedAsync(GameDbContext db)
    {
        if (await db.Words.AnyAsync())
        {
            return;
        }

        db.Words.AddRange(
            new Word { Id = Guid.NewGuid(), Text = "Elefante", Hint = "Animal grande, com memória boa", Category = "Animais" },
            new Word { Id = Guid.NewGuid(), Text = "Guitarra", Hint = "Instrumento com cordas", Category = "Música" },
            new Word { Id = Guid.NewGuid(), Text = "Vulcão", Hint = "Pode entrar em erupção", Category = "Natureza" },
            new Word { Id = Guid.NewGuid(), Text = "Astronauta", Hint = "Trabalha fora da Terra", Category = "Profissões" },
            new Word { Id = Guid.NewGuid(), Text = "Pirâmide", Hint = "Monumento antigo com forma triangular", Category = "Lugares" },
            new Word { Id = Guid.NewGuid(), Text = "Chocolate", Hint = "Doce feito de cacau", Category = "Comida" },
            new Word { Id = Guid.NewGuid(), Text = "Futebol", Hint = "Desporto jogado com os pés", Category = "Desporto" },
            new Word { Id = Guid.NewGuid(), Text = "Biblioteca", Hint = "Lugar cheio de livros", Category = "Lugares" },
            new Word { Id = Guid.NewGuid(), Text = "Dinossauro", Hint = "Animal extinto há milhões de anos", Category = "Animais" },
            new Word { Id = Guid.NewGuid(), Text = "Semáforo", Hint = "Regula o trânsito com cores", Category = "Cidade" }
        );

        await db.SaveChangesAsync();
    }
}
