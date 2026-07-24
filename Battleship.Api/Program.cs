using Battleship.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<BattleshipHub>("/gameHub");

app.Run();