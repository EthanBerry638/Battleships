using System.Collections.Concurrent;
using Battleship.Api.Engine;

namespace Battleship.Api.Services;

public class BattleshipManager (IBattleshipEngineFactory engineFactory)
{
    private readonly ConcurrentDictionary<string, BattleshipEngine> _games = new();
    private readonly IBattleshipEngineFactory _engineFactory = engineFactory;

    public string CreateGame()
    {
        string gameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
        
        var engine = _engineFactory.Create();
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