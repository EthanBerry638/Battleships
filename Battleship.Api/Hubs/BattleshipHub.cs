using Battleship.Api.DTOs;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Battleship.Api.Hubs;

public class BattleshipHub (IBattleshipManager battleshipManager) : Hub 
{
    private readonly IBattleshipManager _battleshipManager = battleshipManager;
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<string> CreateLobby(CreateLobbyRequest request)
    {
        var player = new Player(request.PlayerId, request.PlayerName);
        
        string gameCode = _battleshipManager.CreateLobby(player);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        return gameCode;
    }
    
    public async Task<bool> JoinLobby(string gameCode, JoinLobbyRequest request)
    {
        if (_battleshipManager.GetGame(gameCode) is null) return false;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
        return true;
    }
}