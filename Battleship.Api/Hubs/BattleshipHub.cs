using Battleship.Api.DTOs;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Battleship.Api.Hubs;

public class BattleshipHub (IBattleshipManager battleshipManager) : Hub 
{
    private readonly IBattleshipManager _battleshipManager = battleshipManager;

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string? gameCode =
            await _battleshipManager.HandleDisconnectAsync(Context.ConnectionId, TimeSpan.FromSeconds(30));
        
        if (gameCode is not null)
            await Clients.Group(gameCode).SendCoreAsync("OpponentDisconnected", []);
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<string> CreateLobby(CreateLobbyRequest request)
    {
        var player = new Player(request.PlayerId, request.PlayerName);
        
        string gameCode = _battleshipManager.CreateLobby(player);
        
        _battleshipManager.AddConnection(new AddConnectionRequest(Context.ConnectionId, player.Id));
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        return gameCode;
    }
    
    public async Task<bool> JoinLobby(string gameCode, JoinLobbyRequest request)
    {
        var player = new Player(request.PlayerId, request.PlayerName);
        var engine = _battleshipManager.JoinLobby(gameCode, player);
        
        if (engine is null) return false;
        
        _battleshipManager.AddConnection(new AddConnectionRequest(Context.ConnectionId, player.Id));
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        await Clients.Group(gameCode).SendAsync("GameStarted", engine);
        
        return true;
    }
}