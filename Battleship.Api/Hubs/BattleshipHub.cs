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

    private Guid CallerPlayerId => _connectionService.GetPlayerId(Context.ConnectionId);

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
        _connectionService.EnsureConnectionIsFree(Context.ConnectionId);

        var player = new Player(request.PlayerId, request.PlayerName);

        string gameCode = _sessionService.CreateLobby(player);

        _connectionService.RegisterConnection(Context.ConnectionId, player.Id);
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        return gameCode;
    }
    
    public async Task<bool> JoinLobby(JoinLobbyRequest request)
    {
        _connectionService.EnsureConnectionIsFree(Context.ConnectionId);

        var player = new Player(request.PlayerId, request.PlayerName);
        BattleshipEngine? engine = _sessionService.JoinLobby(request.GameCode, player);

        if (engine is null) return false;

        _connectionService.RegisterConnection(Context.ConnectionId, player.Id);
        await Groups.AddToGroupAsync(Context.ConnectionId, request.GameCode);
        
        var message = new GameCreatedResponse(engine.CurrentPlayer, engine.Players[0].Id, engine.Players[1].Id);
        await Clients.Group(request.GameCode).SendAsync("GameCreated", message);

        return true;
    }

    public async Task LeaveLobby()
    {
        string? gameCode = _sessionService.LeaveLobby(CallerPlayerId);

        if (gameCode is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameCode);
    }

    public PlacementResult PlaceShip(PlaceShipRequest request)
    {
        return _gameService.PlaceShip(CallerPlayerId, request.Type, request.Coordinates);
    }

    public async Task TryStartGame()
    {
        StartGameResponse response = _gameService.TryStartGame(CallerPlayerId);
        
        StartGameMessage message = new(response.IsStarted, response.StartingPlayerId, response.PlayerIds);
        
        if (response.IsStarted)
            await Clients.Group(response.GameCode!).SendAsync("GameStarted", message);
        else
            await Clients.Caller.SendAsync("GameNotStarted", message);
    }

    public FleetValidationResult ValidateFleet()
    {
        return _gameService.ValidateFleet(CallerPlayerId);
    }

    public async Task Shoot(ShootRequest request)
    {
        ShotResponse response = _gameService.Shoot(CallerPlayerId, request.Coordinate);
        
        ShotMessage message = new(response.Result, response.ShooterId, response.Coordinate, response.SunkCoordinates,
            response.IsGameOver);
        
        await Clients.Group(response.GameCode).SendAsync("Shot", message);

        if (response is { IsGameOver: true, PlayerIds: not null })
        {
            foreach (string connectionId in _connectionService.GetConnectionIds(response.PlayerIds))
                await Groups.RemoveFromGroupAsync(connectionId, response.GameCode);
        }
    }

    public void ClearBoard()
    {
        _gameService.ClearBoard(CallerPlayerId);
    }
}