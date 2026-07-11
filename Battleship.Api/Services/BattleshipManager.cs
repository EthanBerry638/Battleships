using System.Collections.Concurrent;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.Engine;

namespace Battleship.Api.Services;

public class BattleshipManager
{
    private readonly ConcurrentDictionary<string, BattleshipEngine> _games = new();

    public string CreateGame()
    {
        string gameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();

        var engine = new BattleshipEngine(new GameBoard(), new GameBoard(), new Player("placeholder"), new Player("placeholder"));
        _games.TryAdd(gameCode, engine);
        
        return gameCode;
    }
    
    public BattleshipEngine GetGame(string gameCode)
    {
        _games.TryGetValue(gameCode, out var engine);
        return engine;
    }
}