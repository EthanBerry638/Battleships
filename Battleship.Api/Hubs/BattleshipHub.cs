using Battleship.Api.DTOs;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Battleship.Api.Hubs;

public class BattleshipHub(IGameService gameService, IConnectionService connectionService, ISessionService sessionService) : Hub
{
    private readonly IGameService _gameService = gameService;
    private readonly IConnectionService _connectionService = connectionService;
    private readonly ISessionService _sessionService = sessionService;

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? gameCode =
            await _connectionService.HandleDisconnectAsync(Context.ConnectionId, TimeSpan.FromSeconds(30));

        if (gameCode is not null)
            await Clients.Group(gameCode).SendAsync("OpponentDisconnected");

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<string> CreateLobby(CreateLobbyRequest request)
    {
        var player = new Player(request.PlayerId, request.PlayerName);

        string gameCode = _sessionService.CreateLobby(player);

        _connectionService.AddConnection(new AddConnectionRequest(Context.ConnectionId, player.Id));
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        return gameCode;
    }

    public async Task<bool> JoinLobby(string gameCode, JoinLobbyRequest request)
    {
        var player = new Player(request.PlayerId, request.PlayerName);
        BattleshipEngine? engine = _sessionService.JoinLobby(gameCode, player);

        if (engine is null) return false;

        _connectionService.AddConnection(new AddConnectionRequest(Context.ConnectionId, player.Id));
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        
        var message = new GameCreatedResponse(engine.CurrentPlayer, engine.Players[0].Id, engine.Players[1].Id);
        await Clients.Group(gameCode).SendAsync("GameStarted", message);

        return true;
    }

    public async Task<PlacementResult> PlaceShip(PlaceShipRequest request)
    {
        PlacementResult result = _gameService.PlaceShip(request);
        await Clients.Caller.SendAsync("PlacementResult", result);
        return result;
    }

    public async Task TryStartGame(Guid playerId)
    {
        throw new NotImplementedException();
    }
}