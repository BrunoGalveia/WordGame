using Microsoft.Extensions.DependencyInjection;
using WordGame.Application.Rooms;

namespace WordGame.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();
        return services;
    }
}
