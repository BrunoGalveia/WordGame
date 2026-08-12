using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordGame.Application.Abstractions;
using WordGame.Infrastructure.Persistence;
using WordGame.Infrastructure.Rooms;

namespace WordGame.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<GameDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
        services.AddScoped<IWordRepository, WordRepository>();

        return services;
    }
}
