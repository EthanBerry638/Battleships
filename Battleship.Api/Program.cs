using Battleship.Api.Hubs;
using Battleship.Api.Repositories;
using Battleship.Api.Services;
using Battleship.Api.DTOs.Validators;
using Battleship.Api.Hubs.Filters;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILobbyRepository, LobbyRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IConnectionRepository, ConnectionRepository>();

builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.AddTransient<IGameService, GameService>();
builder.Services.AddTransient<IConnectionService, ConnectionService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateLobbyRequestValidator>();

builder.Services.AddSignalR(options =>
{
    options.AddFilter<ValidationHubFilter>();         
});

WebApplication app = builder.Build();

app.MapHub<BattleshipHub>("/gameHub");

app.Run();