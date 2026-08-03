using System.Collections.Concurrent;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Services;

namespace Battleship.Api.Repositories;

public class GameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<string, GameSession> _games = new();

    public bool TryAddGame(string gameCode, GameSession session)
    {
        return _games.TryAdd(gameCode, session);
    }

    public bool TryRemoveGame(string gameCode)
    {
        return _games.TryRemove(gameCode, out _);
    }

    public bool TryFindKeyByPlayerId(Guid playerId, out string? gameCode)
    {
        gameCode = _games
            .FirstOrDefault(g => g.Value.Engine.Players
                .Any(p => p.Id == playerId))
            .Key;
        
        return gameCode is not null;    
    }

    public bool TryGetGameByCode(string gameCode, out GameSession? session)
    {
        return _games.TryGetValue(gameCode, out session);   
    }

    public bool IsPlayerInGame(Guid playerId)
    {
        return _games.Values
            .Any(g => g.Engine.Players
                .Any(p => p.Id == playerId));
    }
}