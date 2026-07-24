using Battleship.Api.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

WebApplication app = builder.Build();

app.MapHub<BattleshipHub>("/gameHub");

app.Run();