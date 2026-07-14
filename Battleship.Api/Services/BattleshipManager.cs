using System.Collections.Concurrent;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.GamePieces.Board;

namespace Battleship.Api.Services;

public class BattleshipManager : IBattleshipManager
{
    private readonly ConcurrentDictionary<string, BattleshipEngine> _games = new();
    private readonly ConcurrentDictionary<string, Player> _lobbies = new();

    public string CreateLobby(Player player1)
    {
        ArgumentNullException.ThrowIfNull(player1);
        
        string gameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
        
        _lobbies.TryAdd(gameCode, player1);
        
        return gameCode;
    }
    
    public BattleshipEngine? GetGame(string? gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode)) return null;
        _games.TryGetValue(gameCode, out var engine);
        return engine;
    }
}