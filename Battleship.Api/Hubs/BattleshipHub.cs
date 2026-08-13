using Battleship.Api.DTOs;
using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Responses;
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

        _connectionService.AddConnection(Context.ConnectionId, player.Id);
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        return gameCode;
    }

    // TODO: Fix double return
    public async Task<bool> JoinLobby(JoinLobbyRequest request)
    {
        var player = new Player(request.PlayerId, request.PlayerName);
        BattleshipEngine? engine = _sessionService.JoinLobby(request.GameCode, player);

        if (engine is null) return false;

        _connectionService.AddConnection(Context.ConnectionId, player.Id);
        await Groups.AddToGroupAsync(Context.ConnectionId, request.GameCode);
        
        var message = new GameCreatedResponse(engine.CurrentPlayer, engine.Players[0].Id, engine.Players[1].Id);
        await Clients.Group(request.GameCode).SendAsync("GameCreated", message);

        return true;
    }

    public PlacementResult PlaceShip(PlaceShipRequest request)
    {
        return _gameService.PlaceShip(request);
    }

    public async Task TryStartGame(TryStartGameRequest request)
    {
        StartGameResponse response = _gameService.TryStartGame(request.PlayerId);
        
        if (response.Result.Status is GameStartStatus.Started)
            await Clients.Group(response.GameCode).SendAsync("GameStarted", response.Result);
        else
            await Clients.Caller.SendAsync("GameNotStarted", response.Result);
    }

    public Player? GetWinner(GetWinnerRequest request)
    {
        return _gameService.GetWinner(request.GameCode);
    }
}