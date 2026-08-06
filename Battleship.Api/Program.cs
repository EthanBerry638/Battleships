using Battleship.Api.Hubs;
using Battleship.Api.Repositories;
using Battleship.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILobbyRepository, LobbyRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IConnectionRepository, ConnectionRepository>();

builder.Services.AddTransient<SessionService>();
builder.Services.AddTransient<ConnectionService>();

builder.Services.AddSignalR();

WebApplication app = builder.Build();

app.MapHub<BattleshipHub>("/gameHub");

app.Run();