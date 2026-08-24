using Microsoft.EntityFrameworkCore;
using WordGame.Api.Hubs;
using WordGame.Application;
using WordGame.Application.Rooms;
using WordGame.Infrastructure;
using WordGame.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "Frontend";

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// The frontend's origin(s) — defaults to the Vite dev server; override in production via
// appsettings.Production.json or the Cors__AllowedOrigins__0 environment variable.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
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

app.UseCors(CorsPolicy);
app.UseHttpsRedirection();

app.MapPost("/api/rooms", (IRoomService roomService) => Results.Ok(roomService.CreateRoom()));

app.MapHub<GameHub>("/hubs/game");

app.Run();
