using Battleship.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Battleship.Api.Hubs;

public class BattleshipHub (IBattleshipManager battleshipManager) : Hub 
{
    private readonly IBattleshipManager _manager = battleshipManager;
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
}