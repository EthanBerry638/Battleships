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

    public bool FindKeyByPlayerId(Guid playerId, out string? gameCode)
    {
        throw new NotImplementedException();
    }

    public bool GetByCode(string gameCode, out GameSession? session)
    {
        throw new NotImplementedException();
    }

    public bool IsPlayerInGame(Guid playerId)
    {
        throw new NotImplementedException();
    }
}