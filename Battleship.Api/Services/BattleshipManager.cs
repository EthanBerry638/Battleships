using System.Collections.Concurrent;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.GamePieces.Board;

namespace Battleship.Api.Services;

public class BattleshipManager : IBattleshipManager
{
    private readonly ConcurrentDictionary<string, BattleshipEngine> _games = new();

    public string CreateGame(Player player1, Player player2)
    {
        ArgumentNullException.ThrowIfNull(player1);
        ArgumentNullException.ThrowIfNull(player2);
        
        string gameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();

        var board1 = new GameBoard();
        var board2 = new GameBoard();
        
        var engine = new BattleshipEngine(board1, board2, player1, player2);
        _games.TryAdd(gameCode, engine);
        
        return gameCode;
    }
    
    public BattleshipEngine? GetGame(string? gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode)) return null;
        _games.TryGetValue(gameCode, out var engine);
        return engine;
    }
}