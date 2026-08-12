using Microsoft.EntityFrameworkCore;
using WordGame.Api.Hubs;
using WordGame.Application;
using WordGame.Application.Rooms;
using WordGame.Infrastructure;
using WordGame.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "DevCors";

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<GameDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
    await WordSeeder.SeedAsync(db);
}

app.UseCors(DevCorsPolicy);
app.UseHttpsRedirection();

app.MapPost("/api/rooms", (IRoomService roomService) => Results.Ok(roomService.CreateRoom()));

app.MapHub<GameHub>("/hubs/game");

app.Run();
